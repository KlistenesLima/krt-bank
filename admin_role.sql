ALTER TABLE "Accounts" ADD COLUMN IF NOT EXISTS "Role" VARCHAR(20) DEFAULT 'User';
UPDATE "Accounts" SET "Role" = 'Admin' WHERE "Document" = '44222985007';
SELECT "CustomerName", "Document", "Role" FROM "Accounts" WHERE "Document" = '44222985007';
