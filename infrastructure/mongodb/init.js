db = db.getSiblingDB("clinic_saas_cms_dev");

db.createCollection("tenant_cms_placeholders");
db.tenant_cms_placeholders.createIndex({ tenant_id: 1, type: 1 });
db.tenant_cms_placeholders.createIndex({ tenant_id: 1, updated_at: -1 });
