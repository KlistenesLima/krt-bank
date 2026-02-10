DROP TABLE "PixKeys";
CREATE TABLE "PixKeys" (
    "Id" UUID PRIMARY KEY,
    "AccountId" UUID NOT NULL,
    "KeyType" VARCHAR(10) NOT NULL,
    "KeyValue" VARCHAR(77) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "DeactivatedAt" TIMESTAMP NULL,
    CONSTRAINT fk_pixkeys_account FOREIGN KEY ("AccountId") REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX ix_pixkeys_type_value ON "PixKeys" ("KeyType", "KeyValue") WHERE "IsActive" = true;
CREATE INDEX ix_pixkeys_account_id ON "PixKeys" ("AccountId");

INSERT INTO "PixKeys" ("Id", "AccountId", "KeyType", "KeyValue", "IsActive", "CreatedAt") VALUES
('a1a1a1a1-0001-0001-0001-000000000001', 'cfc2e907-9c0c-4d9e-a7f6-69b958f426d0', 'Cpf', '65414325898', true, NOW()),
('a1a1a1a1-0001-0001-0001-000000000002', 'cfc2e907-9c0c-4d9e-a7f6-69b958f426d0', 'Email', 'ana.silva@krtbank.com', true, NOW()),
('a1a1a1a1-0002-0002-0002-000000000001', '48ce5d3a-4e97-43d5-a629-6330eace620c', 'Cpf', '56889183576', true, NOW()),
('a1a1a1a1-0002-0002-0002-000000000002', '48ce5d3a-4e97-43d5-a629-6330eace620c', 'Email', 'bruno.costa@krtbank.com', true, NOW()),
('a1a1a1a1-0003-0003-0003-000000000001', 'c2b23e01-75d4-4f3a-859d-25c6160a75cd', 'Cpf', '76630448006', true, NOW()),
('a1a1a1a1-0003-0003-0003-000000000002', 'c2b23e01-75d4-4f3a-859d-25c6160a75cd', 'Email', 'diego.oliveira@krtbank.com', true, NOW()),
('a1a1a1a1-0004-0004-0004-000000000001', 'deb3a775-2f9b-4d6e-9ede-5269424ef2f8', 'Cpf', '29302960200', true, NOW()),
('a1a1a1a1-0004-0004-0004-000000000002', 'deb3a775-2f9b-4d6e-9ede-5269424ef2f8', 'Email', 'carla.souza@krtbank.com', true, NOW()),
('a1a1a1a1-0005-0005-0005-000000000001', 'add6660c-f943-40f5-ae53-76ac3dff2ede', 'Cpf', '39880299809', true, NOW()),
('a1a1a1a1-0005-0005-0005-000000000002', 'add6660c-f943-40f5-ae53-76ac3dff2ede', 'Email', 'elena.santos@krtbank.com', true, NOW());
