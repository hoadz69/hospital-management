param(
    [switch]$Once,
    [int]$MaxTasks = 1,
    [switch]$DryRun,
    [switch]$AutoPlan,
    [switch]$PlanOnly,
    [switch]$Continuous,
    [int]$MaxCycles = 1,
    [int]$SleepSeconds = 0,
    [int]$PlannerTimeoutSeconds = 300,
    [int]$WorkerTimeoutSeconds = 900,
    [int]$VerifyTimeoutSeconds = 1800
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Get-Location).Path
$QueuePath = Join-Path $RepoRoot "docs\agent-queue.md"
$PlannerPromptPath = Join-Path $RepoRoot "docs\prompts\AUTO-PLANNER.md"
$RunRoot = Join-Path $RepoRoot "temp\agent-runner"
$ProviderErrorPatterns = @(
    "websocket handshake",
    "handshake failed",
    "model at capacity",
    "rate limit",
    "quota exceeded",
    "failed to connect",
    "provider error",
    "provider failed",
    "codex_api"
)
$WorkerBlockerPatterns = @(
    "CreateProcessWithLogonW failed",
    "Skipped/blocker: Blocker",
    "Skipped/blocker: BLOCKED",
    "Không chạy được runtime smoke",
    "Khong chay duoc runtime smoke",
    "runtime smoke pending",
    "runtime env missing",
    "Verify failed"
)

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message"
}

function Write-Warn {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Fail-Runner {
    param([string]$Message)
    Write-Host "[FAIL] $Message" -ForegroundColor Red
    exit 1
}

function ConvertTo-RepoRelative {
    param([string]$Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $root = [System.IO.Path]::GetFullPath($RepoRoot)
    if ($fullPath.StartsWith($root, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullPath.Substring($root.Length).TrimStart("\", "/").Replace("\", "/")
    }

    return $Path
}

function Assert-RepoRoot {
    if (-not (Test-Path (Join-Path $RepoRoot "AGENTS.md"))) {
        Fail-Runner "Hay chay runner tu repo root co AGENTS.md."
    }

    if (-not (Test-Path $QueuePath)) {
        Fail-Runner "Khong tim thay docs/agent-queue.md."
    }

    $codex = Get-Command codex -ErrorAction SilentlyContinue
    if (-not $codex) {
        Fail-Runner "Khong tim thay command codex trong PATH."
    }

    Write-Info "Repo root: $RepoRoot"
    try {
        $version = (& codex --version 2>$null)
        if ($LASTEXITCODE -eq 0 -and $version) {
            Write-Info "Codex: $version"
        }
    }
    catch {
        Write-Warn "Khong doc duoc codex --version, van tiep tuc vi command ton tai."
    }
}

function Get-QueueText {
    return Get-Content -Raw -Encoding UTF8 $QueuePath
}

function Save-QueueText {
    param([string]$Text)

    $normalized = $Text.TrimEnd() + [Environment]::NewLine
    [System.IO.File]::WriteAllText($QueuePath, $normalized, [System.Text.Encoding]::UTF8)
}

function Stop-ProcessTree {
    param([int]$RootProcessId)

    $children = @(
        Get-CimInstance Win32_Process -ErrorAction SilentlyContinue |
            Where-Object { $_.ParentProcessId -eq $RootProcessId }
    )

    foreach ($child in $children) {
        Stop-ProcessTree -RootProcessId ([int]$child.ProcessId)
    }

    $process = Get-Process -Id $RootProcessId -ErrorAction SilentlyContinue
    if ($process) {
        Stop-Process -Id $RootProcessId -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-ProcessWithTimeout {
    param(
        [string]$FilePath,
        [string[]]$ArgumentList,
        [int]$TimeoutSeconds
    )

    if ($TimeoutSeconds -lt 1) {
        throw "TimeoutSeconds phai >= 1."
    }

    $process = Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -PassThru -WindowStyle Hidden
    $completed = $process.WaitForExit($TimeoutSeconds * 1000)

    if (-not $completed) {
        Stop-ProcessTree -RootProcessId $process.Id
        return [pscustomobject]@{
            ExitCode = 124
            TimedOut = $true
        }
    }

    return [pscustomobject]@{
        ExitCode = [int]$process.ExitCode
        TimedOut = $false
    }
}

function Trim-YamlValue {
    param([string]$Value)

    if ($null -eq $Value) {
        return ""
    }

    $trimmed = $Value.Trim()
    if (($trimmed.StartsWith('"') -and $trimmed.EndsWith('"')) -or
        ($trimmed.StartsWith("'") -and $trimmed.EndsWith("'"))) {
        return $trimmed.Substring(1, $trimmed.Length - 2)
    }

    return $trimmed
}

function Get-YamlScalar {
    param(
        [string]$Yaml,
        [string]$Key
    )

    $escapedKey = [regex]::Escape($Key)
    $match = [regex]::Match($Yaml, "(?m)^$escapedKey\s*:\s*(?<value>.*)$")
    if (-not $match.Success) {
        return ""
    }

    return Trim-YamlValue $match.Groups["value"].Value
}

function Get-YamlInt {
    param(
        [string]$Yaml,
        [string]$Key,
        [int]$Default = 0
    )

    $value = Get-YamlScalar $Yaml $Key
    $number = 0
    if ([int]::TryParse($value, [ref]$number)) {
        return $number
    }

    return $Default
}

function Get-YamlBool {
    param(
        [string]$Yaml,
        [string]$Key,
        [bool]$Default = $false
    )

    $value = (Get-YamlScalar $Yaml $Key).ToLowerInvariant()
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    }

    return $value -in @("true", "yes", "1")
}

function Get-YamlList {
    param(
        [string]$Yaml,
        [string]$Key
    )

    $lines = $Yaml -split "\r?\n"
    $escapedKey = [regex]::Escape($Key)

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        $match = [regex]::Match($line, "^$escapedKey\s*:\s*(?<rest>.*)$")
        if (-not $match.Success) {
            continue
        }

        $rest = $match.Groups["rest"].Value.Trim()
        if ($rest -eq "[]") {
            return @()
        }

        if ($rest -match "^\[(?<items>.*)\]$") {
            $inlineItems = $Matches["items"].Trim()
            if ([string]::IsNullOrWhiteSpace($inlineItems)) {
                return @()
            }

            return @($inlineItems -split "," | ForEach-Object { Trim-YamlValue $_ } | Where-Object { $_ })
        }

        $items = New-Object System.Collections.Generic.List[string]
        for ($j = $i + 1; $j -lt $lines.Count; $j++) {
            $itemMatch = [regex]::Match($lines[$j], "^\s+-\s*(?<item>.*)$")
            if ($itemMatch.Success) {
                $items.Add((Trim-YamlValue $itemMatch.Groups["item"].Value))
                continue
            }

            if ($lines[$j] -match "^\S") {
                break
            }
        }

        return @($items)
    }

    return @()
}

function Get-AgentTasks {
    param([string]$QueueText)

    $pattern = '(?s)<!--\s*task:start\s+(?<marker>[^\r\n]+?)\s*-->\s*```yaml\s*(?<yaml>.*?)```\s*<!--\s*task:end\s*-->'
    $matches = [regex]::Matches($QueueText, $pattern)
    $tasks = New-Object System.Collections.Generic.List[object]

    foreach ($match in $matches) {
        $yaml = $match.Groups["yaml"].Value
        $id = Get-YamlScalar $yaml "id"
        if ([string]::IsNullOrWhiteSpace($id)) {
            $id = $match.Groups["marker"].Value.Trim()
        }

        $tasks.Add([pscustomobject]@{
            Id = $id
            Lane = Get-YamlScalar $yaml "lane"
            Status = (Get-YamlScalar $yaml "status").ToUpperInvariant()
            Priority = Get-YamlInt $yaml "priority" 100
            Title = Get-YamlScalar $yaml "title"
            Executor = (Get-YamlScalar $yaml "executor").ToLowerInvariant()
            PromptFile = Get-YamlScalar $yaml "prompt_file"
            DependsOn = @(Get-YamlList $yaml "depends_on")
            Verify = @(Get-YamlList $yaml "verify")
            AllowedPaths = @(Get-YamlList $yaml "allowed_paths")
            CheckpointFile = Get-YamlScalar $yaml "checkpoint_file"
            BlockerType = (Get-YamlScalar $yaml "blocker_type").ToLowerInvariant()
            AutoRetry = Get-YamlBool $yaml "auto_retry" $false
            Attempts = Get-YamlInt $yaml "attempts" 0
            MaxAttempts = Get-YamlInt $yaml "max_attempts" 1
            RawYaml = $yaml
        })
    }

    return @($tasks.ToArray())
}

function Get-ReadyTasks {
    param([object[]]$Tasks)
    return @($Tasks | Where-Object { $_.Status -eq "READY" } | Sort-Object Priority, Id)
}

function Get-AutoRetryTasks {
    param([object[]]$Tasks)

    return @(
        $Tasks |
            Where-Object {
                $_.Status -eq "BLOCKED" -and
                $_.AutoRetry -and
                $_.BlockerType -in @("env_missing", "auto_recheck") -and
                $_.Attempts -lt $_.MaxAttempts
            } |
            Sort-Object Priority, Id
    )
}

function Test-DependenciesDone {
    param(
        [object]$Task,
        [object[]]$Tasks
    )

    foreach ($dependency in $Task.DependsOn) {
        if ([string]::IsNullOrWhiteSpace($dependency)) {
            continue
        }

        $dependencyTask = $Tasks | Where-Object { $_.Id -eq $dependency } | Select-Object -First 1
        if (-not $dependencyTask) {
            return [pscustomobject]@{
                Done = $false
                Reason = "Dependency $dependency khong ton tai trong queue."
            }
        }

        if ($dependencyTask.Status -ne "DONE") {
            return [pscustomobject]@{
                Done = $false
                Reason = "Dependency $dependency dang la $($dependencyTask.Status), chua DONE."
            }
        }
    }

    return [pscustomobject]@{
        Done = $true
        Reason = ""
    }
}

function Set-TaskStatus {
    param(
        [string]$QueueText,
        [string]$TaskId,
        [string]$Status,
        [string]$Reason = ""
    )

    $escapedId = [regex]::Escape($TaskId)
    $pattern = '(?s)(?<prefix><!--\s*task:start\s+' + $escapedId + '\s*-->\s*```yaml\s*)(?<yaml>.*?)(?<suffix>```\s*<!--\s*task:end\s*-->)'
    $match = [regex]::Match($QueueText, $pattern)
    if (-not $match.Success) {
        throw "Khong tim thay task $TaskId trong queue."
    }

    $yaml = $match.Groups["yaml"].Value
    $yaml = [regex]::Replace($yaml, "(?m)^(started_at|finished_at|result|blocked_reason)\s*:.*\r?\n?", "")

    $timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssK")
    $meta = New-Object System.Collections.Generic.List[string]

    if ($Status -eq "IN_PROGRESS") {
        $meta.Add("started_at: `"$timestamp`"")
    }
    elseif ($Status -in @("DONE", "BLOCKED", "SKIPPED")) {
        $meta.Add("finished_at: `"$timestamp`"")
        $meta.Add("result: `"$Status`"")
        if (-not [string]::IsNullOrWhiteSpace($Reason)) {
            $safeReason = ($Reason -replace "[\r\n]+", " ") -replace '"', "'"
            $meta.Add("blocked_reason: `"$safeReason`"")
        }
    }

    $replacementStatus = "status: $Status"
    if ($meta.Count -gt 0) {
        $replacementStatus = $replacementStatus + "`n" + ($meta -join "`n")
    }

    $yaml = [regex]::Replace(
        $yaml,
        "(?m)^status\s*:.*$",
        { param($m) $replacementStatus },
        1
    )

    $newBlock = $match.Groups["prefix"].Value + $yaml + $match.Groups["suffix"].Value
    return $QueueText.Substring(0, $match.Index) + $newBlock + $QueueText.Substring($match.Index + $match.Length)
}

function Set-TaskScalar {
    param(
        [string]$QueueText,
        [string]$TaskId,
        [string]$Key,
        [string]$Value
    )

    $escapedId = [regex]::Escape($TaskId)
    $pattern = '(?s)(?<prefix><!--\s*task:start\s+' + $escapedId + '\s*-->\s*```yaml\s*)(?<yaml>.*?)(?<suffix>```\s*<!--\s*task:end\s*-->)'
    $match = [regex]::Match($QueueText, $pattern)
    if (-not $match.Success) {
        throw "Khong tim thay task $TaskId trong queue."
    }

    $yaml = $match.Groups["yaml"].Value
    $escapedKey = [regex]::Escape($Key)
    if ($yaml -match "(?m)^$escapedKey\s*:") {
        $yaml = [regex]::Replace($yaml, "(?m)^$escapedKey\s*:.*$", "$Key`: $Value", 1)
    }
    else {
        $yaml = [regex]::Replace($yaml, "(?m)^status\s*:.*$", "`$0`n$Key`: $Value", 1)
    }

    $newBlock = $match.Groups["prefix"].Value + $yaml + $match.Groups["suffix"].Value
    return $QueueText.Substring(0, $match.Index) + $newBlock + $QueueText.Substring($match.Index + $match.Length)
}

function Increment-TaskAttempts {
    param(
        [string]$QueueText,
        [object]$Task
    )

    $nextAttempt = [Math]::Max(0, [int]$Task.Attempts) + 1
    return Set-TaskScalar -QueueText $QueueText -TaskId $Task.Id -Key "attempts" -Value $nextAttempt
}

function Test-ProviderError {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    $lower = $Text.ToLowerInvariant()
    foreach ($pattern in $ProviderErrorPatterns) {
        if ($lower.Contains($pattern)) {
            return "Provider/Codex output contains '$pattern'."
        }
    }

    return ""
}

function Test-WorkerBlocker {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return ""
    }

    foreach ($pattern in $WorkerBlockerPatterns) {
        if ($Text.Contains($pattern)) {
            return "Worker output indicates blocker: $pattern."
        }
    }

    $skippedLine = [regex]::Match($Text, "(?im)^\s*Skipped/blocker:\s*(?<value>.*)$")
    if ($skippedLine.Success) {
        $skippedValue = $skippedLine.Groups["value"].Value.ToLowerInvariant()
        if ($skippedValue.Contains("không có blocker") -or
            $skippedValue.Contains("khong co blocker") -or
            $skippedValue.Trim() -in @("none", "khong", "không")) {
            return ""
        }
    }

    if ($Text -match "(?im)^\s*Skipped/blocker:\s*(?!none\s*$|khong\s*$|không\s*$|không có blocker\b|khong co blocker\b).*(blocked|missing|pending|fail|failed|thiếu|thieu|chưa|chua|không chạy|khong chay)") {
        return "Worker output indicates blocker in Skipped/blocker report."
    }

    if ($Text -match "(?im)^\s*Verify:\s*(không|khong)\s+chạy") {
        return "Worker output indicates verify did not run."
    }

    return ""
}

function Invoke-CodexTask {
    param([object]$Task)

    $promptPath = Join-Path $RepoRoot $Task.PromptFile
    if (-not (Test-Path $promptPath)) {
        throw "Khong tim thay prompt_file $($Task.PromptFile)."
    }

    New-Item -ItemType Directory -Force $RunRoot | Out-Null

    $safeId = $Task.Id -replace "[^A-Za-z0-9._-]", "_"
    $logPath = Join-Path $RunRoot "$safeId.jsonl"
    $lastMessagePath = Join-Path $RunRoot "$safeId.final.md"
    $cmd = 'codex exec -C "' + $RepoRoot + '" --json -o "' + $lastMessagePath + '" - < "' + $promptPath + '" > "' + $logPath + '" 2>&1'

    Write-Info "Running codex exec for $($Task.Id)."
    Write-Info "Log: $(ConvertTo-RepoRelative $logPath)"

    $process = Invoke-ProcessWithTimeout -FilePath "cmd.exe" -ArgumentList @("/d", "/c", $cmd) -TimeoutSeconds $WorkerTimeoutSeconds

    $combinedOutput = ""
    $finalOutput = ""
    if (Test-Path $logPath) {
        $combinedOutput += Get-Content -Raw -Encoding UTF8 $logPath
    }
    if (Test-Path $lastMessagePath) {
        $finalOutput = Get-Content -Raw -Encoding UTF8 $lastMessagePath
        $combinedOutput += "`n" + $finalOutput
    }

    $providerError = Test-ProviderError $finalOutput
    if (-not $process.TimedOut -and $process.ExitCode -ne 0 -and [string]::IsNullOrWhiteSpace($providerError)) {
        $providerError = Test-ProviderError $combinedOutput
    }
    $workerBlocker = Test-WorkerBlocker $finalOutput
    return [pscustomobject]@{
        ExitCode = $process.ExitCode
        TimedOut = $process.TimedOut
        LogPath = $logPath
        LastMessagePath = $lastMessagePath
        ProviderError = $providerError
        WorkerBlocker = $workerBlocker
    }
}

function Invoke-AutoPlanner {
    if (-not (Test-Path $PlannerPromptPath)) {
        throw "Khong tim thay docs/prompts/AUTO-PLANNER.md."
    }

    New-Item -ItemType Directory -Force $RunRoot | Out-Null

    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $logPath = Join-Path $RunRoot "AUTO-PLANNER.$stamp.jsonl"
    $lastMessagePath = Join-Path $RunRoot "AUTO-PLANNER.$stamp.final.md"
    $beforeQueue = Get-QueueText
    $cmd = 'codex exec -C "' + $RepoRoot + '" --json -o "' + $lastMessagePath + '" - < "' + $PlannerPromptPath + '" > "' + $logPath + '" 2>&1'

    Write-Info "Running auto planner."
    Write-Info "Log: $(ConvertTo-RepoRelative $logPath)"

    $process = Invoke-ProcessWithTimeout -FilePath "cmd.exe" -ArgumentList @("/d", "/c", $cmd) -TimeoutSeconds $PlannerTimeoutSeconds

    $combinedOutput = ""
    $finalOutput = ""
    if (Test-Path $logPath) {
        $combinedOutput += Get-Content -Raw -Encoding UTF8 $logPath
    }
    if (Test-Path $lastMessagePath) {
        $finalOutput = Get-Content -Raw -Encoding UTF8 $lastMessagePath
        $combinedOutput += "`n" + $finalOutput
    }

    $providerError = Test-ProviderError $finalOutput
    if (-not $process.TimedOut -and $process.ExitCode -ne 0 -and [string]::IsNullOrWhiteSpace($providerError)) {
        $providerError = Test-ProviderError $combinedOutput
    }

    $afterQueue = Get-QueueText
    return [pscustomobject]@{
        ExitCode = $process.ExitCode
        TimedOut = $process.TimedOut
        LogPath = $logPath
        LastMessagePath = $lastMessagePath
        ProviderError = $providerError
        QueueChanged = ($beforeQueue -ne $afterQueue)
    }
}

function Add-FallbackSmokeTask {
    $queueText = Get-QueueText
    if ($queueText.Contains("<!-- task:start AUTO-RUNNER-V2-SMOKE -->")) {
        Write-Info "Fallback smoke task already exists."
        return $false
    }

    $promptPath = Join-Path $RepoRoot "docs\prompts\AUTO-RUNNER-V2-SMOKE.md"
    if (-not (Test-Path $promptPath)) {
        $prompt = @'
# AUTO-RUNNER-V2-SMOKE

Lead Agent: chạy smoke task an toàn cho Agent Runner V2.

## Mode

- Docs/workflow smoke only.
- No commit.
- No push.
- No stage.
- No backend/frontend runtime source changes.
- Do not use shell commands. Use `apply_patch` only if editing is needed.

## Scope

Append a short checkpoint to `docs/current-task.md` confirming Agent Runner V2 could run a generated queue task.

## Allowed Files

- `docs/current-task.md`
- `docs/agent-queue.md`

## Implement

Add one short section near the end of `docs/current-task.md`:

```md
## Agent Runner V2 Smoke - 2026-05-14

- Result: generated queue task executed by runner.
- Scope: docs-only smoke, no source runtime changes.
- Guardrail: no commit/push/stage.
```

Do not edit backend, frontend, migrations, Figma, screenshots, or logs.

## Verify

Outer runner will run:

```txt
git diff --check
```

## Stop If

- You cannot edit `docs/current-task.md`.
- Any source runtime file would need changes.

## Report

```txt
Lane:
Action:
Files changed:
Verify:
Skipped/blocker:
Dirty:
Next:
```
'@

        Set-Content -Path $promptPath -Value $prompt -Encoding UTF8
    }

    $taskBlock = @'

<!-- task:start AUTO-RUNNER-V2-SMOKE -->
```yaml
id: AUTO-RUNNER-V2-SMOKE
lane: docs
status: READY
priority: 1
title: Agent Runner V2 generated smoke task
prompt_file: docs/prompts/AUTO-RUNNER-V2-SMOKE.md
depends_on: []
blocker_type: none
auto_retry: false
attempts: 0
max_attempts: 1
allowed_paths:
  - docs/current-task.md
  - docs/agent-queue.md
verify:
  - git diff --check
stop_conditions:
  - codex exec failed
  - verify failed
checkpoint_file: docs/current-task.md
```
<!-- task:end -->
'@

    $updatedQueue = $queueText.TrimEnd() + "`r`n" + $taskBlock.TrimStart() + "`r`n"
    Save-QueueText $updatedQueue
    Write-Info "Fallback smoke task added to queue."
    return $true
}

function Add-FallbackSelfCheckTask {
    $queueText = Get-QueueText
    if ($queueText.Contains("<!-- task:start AUTO-RUNNER-SELF-CHECK -->")) {
        $tasks = @(Get-AgentTasks $queueText)
        $task = $tasks | Where-Object { $_.Id -eq "AUTO-RUNNER-SELF-CHECK" } | Select-Object -First 1
        if ($task -and $task.Status -eq "READY") {
            Write-Info "Fallback self-check task already READY."
            return $true
        }

        $queueText = Set-TaskStatus -QueueText $queueText -TaskId "AUTO-RUNNER-SELF-CHECK" -Status "READY"
        $queueText = Set-TaskScalar -QueueText $queueText -TaskId "AUTO-RUNNER-SELF-CHECK" -Key "attempts" -Value "0"
        Save-QueueText $queueText
        Write-Info "Fallback self-check task reactivated."
        return $true
    }

    $taskBlock = @'

<!-- task:start AUTO-RUNNER-SELF-CHECK -->
```yaml
id: AUTO-RUNNER-SELF-CHECK
lane: docs
status: READY
priority: 2
title: Agent Runner verify-only self check
executor: verify_only
prompt_file: ""
depends_on: []
blocker_type: none
auto_retry: false
attempts: 0
max_attempts: 1
allowed_paths:
  - docs/agent-queue.md
  - docs/current-task.md
  - scripts/agent-runner.ps1
verify:
  - git diff --check
stop_conditions:
  - verify failed
checkpoint_file: docs/current-task.md
```
<!-- task:end -->
'@

    $updatedQueue = $queueText.TrimEnd() + "`r`n" + $taskBlock.TrimStart() + "`r`n"
    Save-QueueText $updatedQueue
    Write-Info "Fallback self-check task added to queue."
    return $true
}

function Test-FileContainsText {
    param(
        [string]$RelativePath,
        [string]$Needle
    )

    $path = Join-Path $RepoRoot $RelativePath
    if (-not (Test-Path $path)) {
        return $false
    }

    $content = Get-Content -Raw -Encoding UTF8 $path
    return $content.Contains($Needle)
}

function Add-DeterministicLifecycleApiWiringTask {
    $taskId = "FE-TENANT-LIFECYCLE-API-WIRING"
    $queueText = Get-QueueText

    if ($queueText.Contains("<!-- task:start $taskId -->")) {
        $tasks = @(Get-AgentTasks $queueText)
        $task = $tasks | Where-Object { $_.Id -eq $taskId } | Select-Object -First 1
        if ($task -and $task.Status -eq "READY") {
            Write-Info "Deterministic task already READY: $taskId."
            return $true
        }

        Write-Info "Deterministic task already exists with status $($task.Status): $taskId."
        return $false
    }

    $hasMockLifecycle = (Test-FileContainsText -RelativePath "frontend\apps\owner-admin\src\pages\ClinicDetailPage.vue" -Needle "lifecycleTimer = window.setTimeout") -and
        (Test-FileContainsText -RelativePath "frontend\apps\owner-admin\src\pages\ClinicDetailPage.vue" -Needle "mock-local")
    $hasFrontendClient = (Test-FileContainsText -RelativePath "frontend\packages\api-client\src\tenantClient.ts" -Needle "updateTenantStatus(tenantId") -and
        (Test-FileContainsText -RelativePath "frontend\packages\api-client\src\tenantClient.ts" -Needle "/api/tenants/`${tenantId}/status")
    $hasBackendStatusRoute = (Test-FileContainsText -RelativePath "backend\services\api-gateway\src\ApiGateway.Api\Endpoints\TenantEndpoints.cs" -Needle 'MapPatch("/{tenantId:guid}/status"') -or
        (Test-FileContainsText -RelativePath "backend\services\tenant-service\src\TenantService.Api\Endpoints\TenantEndpoints.cs" -Needle 'MapPatch("/{tenantId:guid}/status"')

    if (-not ($hasMockLifecycle -and $hasFrontendClient -and $hasBackendStatusRoute)) {
        Write-Info "Deterministic lifecycle task not applicable. mock=$hasMockLifecycle client=$hasFrontendClient backend=$hasBackendStatusRoute"
        return $false
    }

    $promptPath = Join-Path $RepoRoot "docs\prompts\$taskId.md"
    $prompt = @'
# FE-TENANT-LIFECYCLE-API-WIRING

Lead Agent: implement frontend-only tenant lifecycle API wiring cho Owner Admin.

## Mode

- Lane frontend.
- Khong commit, push, stage.
- Khong sua backend, database, infrastructure, Figma, screenshot, hoac generated artifact.
- Chi lam trong Allowed Files.

## Read

- `AGENTS.md`
- `docs/current-task.md`
- `docs/current-task.frontend.md`
- `temp/plan.frontend.md`
- `rules/coding-rules.md`
- Source frontend lien quan trong Allowed Files.

## Context

`/clinics/:tenantId` da co `TenantLifecycleConfirmModal`, nhung `ClinicDetailPage.vue` van dung `lifecycleTimer = window.setTimeout(...)`, local status mutation, va audit metadata `mock-local`. Shared API client da co `tenantClient.updateTenantStatus(tenantId, { status })`, backend/gateway da co `PATCH /api/tenants/{tenantId}/status`.

## Scope

1. Sua `frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue` de `confirmLifecycle(action, reason)` goi `tenantClient.updateTenantStatus(tenant.value.id, { status })` thay cho local timer/mock status mutation.
2. Giu behavior modal: loading, success, error, tu dong dong sau success, reason text trong audit event local.
3. Dung `TenantDetail` tra ve tu `updateTenantStatus` lam source of truth cho `tenant.value`.
4. Doi copy va audit metadata trong lifecycle path tu mock/local sang API-first. Khong gui reason len backend vi contract hien tai chi ho tro `{ status }`.
5. Chi xoa lifecycle timer cleanup neu no thanh dead code. Khong dong vao audit/domain timers neu khong can cho typecheck.
6. Sua copy trong `TenantLifecycleConfirmModal.vue` neu con noi "mock lifecycle" hoac "Khong goi API lifecycle moi", de phu hop hanh vi status update qua API ma khong claim audit persistence.
7. Neu verify pass, cap nhat ngan `docs/current-task.frontend.md` va `temp/plan.frontend.md`.

## Allowed Files

- `frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue`
- `frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue`
- `docs/current-task.frontend.md`
- `temp/plan.frontend.md`

## Verify

Outer runner se chay:

```txt
git diff --check
cd frontend && npm run typecheck
cd frontend && npm run build
```

## Stop If

- `tenantClient.updateTenantStatus` thieu hoac khong goi `/api/tenants/{tenantId}/status`.
- Backend status route khong ton tai.
- Scope bat buoc persist lifecycle reason server-side. Scope hien tai chi la status API wiring.
- Can sua backend/API contract.

## Report

```txt
Lane:
Action:
Files changed:
Verify:
Skipped/blocker:
Dirty:
Next:
```
'@

    [System.IO.File]::WriteAllText($promptPath, $prompt, [System.Text.Encoding]::UTF8)

    $taskBlock = @'

<!-- task:start FE-TENANT-LIFECYCLE-API-WIRING -->
```yaml
id: FE-TENANT-LIFECYCLE-API-WIRING
lane: frontend
status: READY
priority: 1
title: Owner Admin tenant lifecycle API wiring
executor: codex
prompt_file: docs/prompts/FE-TENANT-LIFECYCLE-API-WIRING.md
depends_on: []
blocker_type: none
auto_retry: false
attempts: 0
max_attempts: 1
allowed_paths:
  - frontend/apps/owner-admin/src/pages/ClinicDetailPage.vue
  - frontend/apps/owner-admin/src/components/TenantLifecycleConfirmModal.vue
  - docs/current-task.frontend.md
  - temp/plan.frontend.md
verify:
  - git diff --check
  - cd frontend && npm run typecheck
  - cd frontend && npm run build
stop_conditions:
  - codex exec failed
  - codex exec timeout
  - verify failed
  - backend contract missing
checkpoint_file: docs/current-task.frontend.md
```
<!-- task:end -->
'@

    $updatedQueue = $queueText.TrimEnd() + "`r`n" + $taskBlock.TrimStart() + "`r`n"
    Save-QueueText $updatedQueue
    Write-Info "Deterministic product task added to queue: $taskId."
    return $true
}

function Add-DeterministicProductTask {
    if (Add-DeterministicLifecycleApiWiringTask) {
        return $true
    }

    return $false
}

function Add-DeterministicFrontendDirtyVerifyTask {
    $taskId = "FE-DIRTY-VERIFY"
    $queueText = Get-QueueText

    if ($queueText.Contains("<!-- task:start $taskId -->")) {
        $tasks = @(Get-AgentTasks $queueText)
        $task = $tasks | Where-Object { $_.Id -eq $taskId } | Select-Object -First 1
        if ($task -and $task.Status -eq "READY") {
            Write-Info "Deterministic verify task already READY: $taskId."
            return $true
        }

        if ($task -and $task.Status -eq "DONE") {
            Write-Info "Deterministic verify task already DONE: $taskId."
            return $false
        }
    }

    $dirty = @(& git status --short -- frontend)
    if (-not $dirty -or $dirty.Count -eq 0) {
        Write-Info "No frontend dirty files for deterministic verify task."
        return $false
    }

    $taskBlock = @'

<!-- task:start FE-DIRTY-VERIFY -->
```yaml
id: FE-DIRTY-VERIFY
lane: frontend
status: READY
priority: 3
title: Verify current dirty frontend changes
executor: verify_only
prompt_file: ""
depends_on: []
blocker_type: none
auto_retry: false
attempts: 0
max_attempts: 1
allowed_paths:
  - frontend/**
  - docs/current-task.frontend.md
  - temp/plan.frontend.md
verify:
  - git diff --check
  - cd frontend && npm run typecheck
  - cd frontend && npm run build
  - gitnexus detect-changes --scope all
stop_conditions:
  - verify failed
checkpoint_file: docs/current-task.frontend.md
```
<!-- task:end -->
'@

    $updatedQueue = $queueText.TrimEnd() + "`r`n" + $taskBlock.TrimStart() + "`r`n"
    Save-QueueText $updatedQueue
    Write-Info "Deterministic frontend verify task added to queue: $taskId."
    return $true
}

function Add-DeterministicNextTask {
    if (Add-DeterministicProductTask) {
        return $true
    }

    if (Add-DeterministicFrontendDirtyVerifyTask) {
        return $true
    }

    return $false
}

function Invoke-ExternalCommand {
    param(
        [string]$Command,
        [string]$OutputPath
    )

    $cmdLine = ""
    $dotnetCommand = "dotnet"
    $dotnetFromPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnetFromPath) {
        $dotnetCommand = '"' + $dotnetFromPath.Source + '"'
    }
    elseif (Test-Path "D:\tools\dotnet\dotnet.exe") {
        $dotnetCommand = '"D:\tools\dotnet\dotnet.exe"'
    }
    elseif (Test-Path "C:\Program Files\dotnet\dotnet.exe") {
        $dotnetCommand = '"C:\Program Files\dotnet\dotnet.exe"'
    }

    switch ($Command) {
        "git diff --check" {
            $cmdLine = 'git diff --check'
        }
        "cd frontend && npm run typecheck" {
            $cmdLine = 'cd /d "' + (Join-Path $RepoRoot "frontend") + '" && npm run typecheck'
        }
        "cd frontend && npm run build" {
            $cmdLine = 'cd /d "' + (Join-Path $RepoRoot "frontend") + '" && npm run build'
        }
        "frontend owner-admin route smoke" {
            $cmdLine = 'powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\verify-owner-admin-ui-smoke.ps1"'
        }
        "dotnet restore backend/ClinicSaaS.Backend.sln" {
            $cmdLine = $dotnetCommand + ' restore "backend/ClinicSaaS.Backend.sln"'
        }
        "dotnet build backend/ClinicSaaS.Backend.sln --no-restore" {
            $cmdLine = $dotnetCommand + ' build "backend/ClinicSaaS.Backend.sln" --no-restore'
        }
        "dotnet test backend/ClinicSaaS.Backend.sln --no-build" {
            $cmdLine = $dotnetCommand + ' test "backend/ClinicSaaS.Backend.sln" --no-build'
        }
        "docker compose -f infrastructure/docker/docker-compose.dev.yml config" {
            $cmdLine = 'docker compose -f "infrastructure/docker/docker-compose.dev.yml" config'
        }
        "test backend domain runtime env" {
            $localDeploy = Join-Path $RepoRoot "deploy.local.ps1"
            $escapedDeploy = $localDeploy.Replace("'", "''")
            $script = @(
                '$ErrorActionPreference = "Continue"',
                "if (Test-Path '$escapedDeploy') { . '$escapedDeploy' }",
                '$dockerReady = $false',
                'try { docker info *> $null; if ($LASTEXITCODE -eq 0) { $dockerReady = $true } } catch { }',
                'if ($dockerReady) { Write-Output "Docker daemon available for backend runtime smoke."; exit 0 }',
                '$serverReady = -not [string]::IsNullOrWhiteSpace($env:DEPLOY_HOST) -and -not [string]::IsNullOrWhiteSpace($env:DEPLOY_USER) -and -not [string]::IsNullOrWhiteSpace($env:SSH_KEY_PATH) -and (Test-Path $env:SSH_KEY_PATH)',
                'if ($serverReady) { Write-Output "Server smoke env available for backend runtime smoke."; exit 0 }',
                'Write-Output "Runtime smoke env missing: Docker daemon unavailable and DEPLOY_HOST/DEPLOY_USER/SSH_KEY_PATH not ready."',
                'exit 1'
            ) -join "; "
            $cmdLine = 'powershell -NoProfile -ExecutionPolicy Bypass -Command "' + ($script -replace '"', '\"') + '"'
        }
        "gitnexus detect-changes --scope all" {
            $cmdLine = 'gitnexus detect-changes --scope all'
        }
        default {
            "Unsupported verify command: $Command" | Set-Content -Path $OutputPath -Encoding UTF8
            return 127
        }
    }

    $fullCmd = 'cd /d "' + $RepoRoot + '" && ' + $cmdLine + ' > "' + $OutputPath + '" 2>&1'
    $process = Invoke-ProcessWithTimeout -FilePath "cmd.exe" -ArgumentList @("/d", "/c", $fullCmd) -TimeoutSeconds $VerifyTimeoutSeconds
    if ($process.TimedOut) {
        "Verify command timed out after $VerifyTimeoutSeconds seconds: $Command" | Add-Content -Path $OutputPath -Encoding UTF8
    }

    return [int]$process.ExitCode
}

function Invoke-VerifyCommands {
    param([object]$Task)

    New-Item -ItemType Directory -Force $RunRoot | Out-Null

    $safeId = $Task.Id -replace "[^A-Za-z0-9._-]", "_"
    $verifyLogPath = Join-Path $RunRoot "$safeId.verify.log"
    Set-Content -Path $verifyLogPath -Value "" -Encoding UTF8

    if (-not $Task.Verify -or $Task.Verify.Count -eq 0) {
        Add-Content -Path $verifyLogPath -Value "No verify commands declared." -Encoding UTF8
        return [pscustomobject]@{
            Success = $true
            LogPath = $verifyLogPath
            Summary = "No verify commands declared."
        }
    }

    foreach ($command in $Task.Verify) {
        if ([string]::IsNullOrWhiteSpace($command)) {
            continue
        }

        Write-Info "Verify: $command"
        $stepLog = Join-Path $RunRoot "$safeId.verify.step.log"
        if (Test-Path $stepLog) {
            Remove-Item -LiteralPath $stepLog -Force
        }

        Add-Content -Path $verifyLogPath -Value "## $command" -Encoding UTF8
        $exitCode = Invoke-ExternalCommand -Command $command -OutputPath $stepLog

        if (Test-Path $stepLog) {
            Get-Content -Raw -Encoding UTF8 $stepLog | Add-Content -Path $verifyLogPath -Encoding UTF8
        }

        Add-Content -Path $verifyLogPath -Value "exit_code: $exitCode`n" -Encoding UTF8

        if ($exitCode -ne 0) {
            return [pscustomobject]@{
                Success = $false
                LogPath = $verifyLogPath
                Summary = "Verify failed: $command"
            }
        }
    }

    return [pscustomobject]@{
        Success = $true
        LogPath = $verifyLogPath
        Summary = "PASS"
    }
}

function Add-Checkpoint {
    param(
        [object]$Task,
        [string]$Result,
        [string]$Reason,
        [string]$LogPath,
        [string]$VerifySummary
    )

    if ([string]::IsNullOrWhiteSpace($Task.CheckpointFile)) {
        return
    }

    $checkpointPath = Join-Path $RepoRoot $Task.CheckpointFile
    if (-not (Test-Path $checkpointPath)) {
        Write-Warn "Checkpoint file khong ton tai: $($Task.CheckpointFile)."
        return
    }

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
    $relativeLog = if ($LogPath) { ConvertTo-RepoRelative $LogPath } else { "n/a" }
    $reasonLine = if ([string]::IsNullOrWhiteSpace($Reason)) { "none" } else { $Reason }

    $checkpoint = @(
        "",
        "## Agent Runner Checkpoint - $timestamp",
        "",
        ('- Task id: `{0}`' -f $Task.Id),
        ('- Lane: `{0}`' -f $Task.Lane),
        ('- Result: `{0}`' -f $Result),
        "- Reason: $reasonLine",
        ('- Log: `{0}`' -f $relativeLog),
        "- Verify: $VerifySummary",
        "- Guardrail: runner khong commit/push/stage."
    )

    Add-Content -Path $checkpointPath -Value $checkpoint -Encoding UTF8
}

function Get-SelectedTask {
    param([object[]]$Tasks)

    $readyTasks = Get-ReadyTasks $Tasks
    foreach ($task in $readyTasks) {
        $dependencyResult = Test-DependenciesDone -Task $task -Tasks $Tasks
        if ($dependencyResult.Done) {
            return [pscustomobject]@{
                Task = $task
                BlockReason = ""
            }
        }

        Write-Warn "Skip $($task.Id): $($dependencyResult.Reason)"
    }

    return [pscustomobject]@{
        Task = $null
        BlockReason = "No READY task with dependencies DONE."
    }
}

function Main {
    if ($Once) {
        $MaxTasks = 1
    }

    if ($Continuous -and -not $Once -and $MaxTasks -eq 1) {
        $MaxTasks = [Math]::Max(1, $MaxCycles)
    }

    if ($MaxTasks -lt 1) {
        Fail-Runner "MaxTasks phai >= 1."
    }

    if ($MaxCycles -lt 1) {
        Fail-Runner "MaxCycles phai >= 1."
    }

    if ($SleepSeconds -lt 0) {
        Fail-Runner "SleepSeconds phai >= 0."
    }

    if ($PlannerTimeoutSeconds -lt 1 -or $WorkerTimeoutSeconds -lt 1 -or $VerifyTimeoutSeconds -lt 1) {
        Fail-Runner "PlannerTimeoutSeconds, WorkerTimeoutSeconds va VerifyTimeoutSeconds phai >= 1."
    }

    Assert-RepoRoot

    $initialStatus = (& git status --short)
    if ($initialStatus) {
        Write-Warn "Repo dang dirty truoc khi runner chay. Runner se khong revert thay doi co san."
        $initialStatus | ForEach-Object { Write-Warn $_ }
    }

    $completed = 0
    $plannerRuns = 0
    $summary = New-Object System.Collections.Generic.List[string]

    while ($completed -lt $MaxTasks) {
        $queueText = Get-QueueText
        $tasks = @(Get-AgentTasks $queueText)

        $selection = Get-SelectedTask $tasks
        if (-not $selection.Task) {
            if ($tasks -and $tasks.Count -gt 0) {
                $retryTasks = @(Get-AutoRetryTasks $tasks)
                if ($retryTasks.Count -gt 0) {
                    $retryTask = $retryTasks | Select-Object -First 1
                    Write-Info "Auto re-check task: $($retryTask.Id) [$($retryTask.BlockerType)] attempt $($retryTask.Attempts + 1)/$($retryTask.MaxAttempts)."
                    if ($DryRun) {
                        Write-Info "DryRun: would reactivate blocked task $($retryTask.Id)."
                        $summary.Add("DRYRUN auto-recheck $($retryTask.Id)")
                        break
                    }

                    $queueText = Set-TaskStatus -QueueText $queueText -TaskId $retryTask.Id -Status "READY"
                    Save-QueueText $queueText
                    continue
                }
            }

            if ($AutoPlan -and $plannerRuns -lt $MaxCycles) {
                if ($DryRun) {
                    Write-Info "DryRun: would run auto planner because no READY task is available."
                    $summary.Add("DRYRUN auto-plan")
                    break
                }

                try {
                    $plannerResult = Invoke-AutoPlanner
                    $plannerRuns++

                    if ($plannerResult.TimedOut) {
                        $reason = "auto planner timed out after $PlannerTimeoutSeconds seconds."
                        Write-Warn $reason
                        $fallbackAdded = Add-DeterministicNextTask
                        if (-not $fallbackAdded) {
                            $fallbackAdded = Add-FallbackSelfCheckTask
                        }
                        if ($fallbackAdded) {
                            $summary.Add("AUTO-PLAN deterministic fallback: $reason")
                            if ($PlanOnly) {
                                break
                            }

                            continue
                        }

                        $summary.Add("BLOCKED auto-plan: $reason")
                        break
                    }

                    if ($plannerResult.ExitCode -ne 0) {
                        $reason = "auto planner failed with exit code $($plannerResult.ExitCode)."
                        Write-Warn $reason
                        $fallbackAdded = Add-DeterministicNextTask
                        if (-not $fallbackAdded) {
                            $fallbackAdded = Add-FallbackSelfCheckTask
                        }
                        if ($fallbackAdded) {
                            $summary.Add("AUTO-PLAN deterministic fallback: $reason")
                            if ($PlanOnly) {
                                break
                            }

                            continue
                        }

                        $summary.Add("BLOCKED auto-plan: $reason")
                        break
                    }

                    if (-not [string]::IsNullOrWhiteSpace($plannerResult.ProviderError)) {
                        Write-Warn $plannerResult.ProviderError
                        $fallbackAdded = Add-DeterministicNextTask
                        if (-not $fallbackAdded) {
                            $fallbackAdded = Add-FallbackSelfCheckTask
                        }
                        if ($fallbackAdded) {
                            $summary.Add("AUTO-PLAN deterministic fallback: $($plannerResult.ProviderError)")
                            if ($PlanOnly) {
                                break
                            }

                            continue
                        }

                        $summary.Add("BLOCKED auto-plan: $($plannerResult.ProviderError)")
                        break
                    }

                    if ($plannerResult.QueueChanged) {
                        Write-Info "Auto planner updated queue."
                    }
                    else {
                        Write-Warn "Auto planner completed but queue did not change."
                        $fallbackAdded = Add-DeterministicNextTask
                        if ($fallbackAdded) {
                            Write-Info "Deterministic fallback planner created a next task."
                        }
                    }

                    $summary.Add("AUTO-PLAN")

                    if ($PlanOnly) {
                        break
                    }

                    if ($SleepSeconds -gt 0) {
                        Start-Sleep -Seconds $SleepSeconds
                    }

                    continue
                }
                catch {
                    $reason = $_.Exception.Message
                    Write-Warn "Auto planner BLOCKED: $reason"
                    $summary.Add("BLOCKED auto-plan: $reason")
                    break
                }
            }

            Write-Info "No READY task."
            break
        }

        $task = $selection.Task
        if ([string]::IsNullOrWhiteSpace($task.Executor)) {
            $task.Executor = "codex"
        }

        Write-Info "Selected task: $($task.Id) [$($task.Lane)] $($task.Title)"

        if ($DryRun) {
            Write-Info "DryRun: would run prompt $($task.PromptFile)"
            if ($task.DependsOn.Count -gt 0) {
                Write-Info "Depends on: $($task.DependsOn -join ', ')"
            }
            if ($task.Verify.Count -gt 0) {
                Write-Info "Verify: $($task.Verify -join ' | ')"
            }
            $summary.Add("DRYRUN $($task.Id)")
            break
        }

        $queueText = Set-TaskStatus -QueueText $queueText -TaskId $task.Id -Status "IN_PROGRESS"
        $queueText = Increment-TaskAttempts -QueueText $queueText -Task $task
        Save-QueueText $queueText

        $result = "BLOCKED"
        $reason = ""
        $logPath = ""
        $verifySummary = "not run"

        try {
            if ($task.Executor -eq "verify_only") {
                Write-Info "Executor verify_only: skip codex exec for $($task.Id)."
            }
            else {
                $codexResult = Invoke-CodexTask -Task $task
                $logPath = $codexResult.LogPath

                if ($codexResult.TimedOut) {
                    $reason = "codex exec timed out after $WorkerTimeoutSeconds seconds."
                }
                elseif ($codexResult.ExitCode -ne 0) {
                    $reason = "codex exec failed with exit code $($codexResult.ExitCode)."
                }
                elseif (-not [string]::IsNullOrWhiteSpace($codexResult.ProviderError)) {
                    $reason = $codexResult.ProviderError
                }
                elseif (-not [string]::IsNullOrWhiteSpace($codexResult.WorkerBlocker)) {
                    $reason = $codexResult.WorkerBlocker
                }
            }

            if ([string]::IsNullOrWhiteSpace($reason)) {
                $verifyResult = Invoke-VerifyCommands -Task $task
                $verifySummary = $verifyResult.Summary

                if (-not $verifyResult.Success) {
                    $reason = $verifyResult.Summary
                    $logPath = $verifyResult.LogPath
                }
                else {
                    $result = "DONE"
                }
            }
        }
        catch {
            $reason = $_.Exception.Message
        }

        $queueText = Get-QueueText
        if ($result -eq "DONE") {
            $queueText = Set-TaskStatus -QueueText $queueText -TaskId $task.Id -Status "DONE"
            Save-QueueText $queueText
            Add-Checkpoint -Task $task -Result "DONE" -Reason "" -LogPath $logPath -VerifySummary $verifySummary
            Write-Info "Task DONE: $($task.Id)"
            $summary.Add("DONE $($task.Id)")
        }
        else {
            $queueText = Set-TaskStatus -QueueText $queueText -TaskId $task.Id -Status "BLOCKED" -Reason $reason
            Save-QueueText $queueText
            Add-Checkpoint -Task $task -Result "BLOCKED" -Reason $reason -LogPath $logPath -VerifySummary $verifySummary
            Write-Warn "Task BLOCKED: $($task.Id) - $reason"
            $summary.Add("BLOCKED $($task.Id): $reason")
            break
        }

        $dirty = (& git status --short)
        if ($dirty) {
            Write-Info "Dirty summary after task:"
            $dirty | ForEach-Object { Write-Host $_ }
        }

        $completed++
    }

    Write-Info "Runner summary:"
    if ($summary.Count -eq 0) {
        Write-Host "No task executed."
    }
    else {
        $summary | ForEach-Object { Write-Host "- $_" }
    }
}

Main
