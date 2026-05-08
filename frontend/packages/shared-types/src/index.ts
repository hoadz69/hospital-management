export type TenantSummary = {
  id: string;
  name: string;
  subdomain: string;
};

export const mockTenant: TenantSummary = {
  id: "tenant-demo",
  name: "MediCare+ Demo Clinic",
  subdomain: "medicare-demo"
};
