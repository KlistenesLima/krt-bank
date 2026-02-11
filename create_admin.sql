INSERT INTO "Accounts" ("Id", "CustomerName", "Document", "Email", "Phone", "Balance", "Status", "Type", "Role", "RowVersion", "CreatedAt", "UpdatedAt")
VALUES (
    'a1b2c3d4-0000-0000-0000-aabbccddeeff',
    'Klistenes Lima',
    '44222985007',
    'klistenes@krtbank.com',
    '83999999999',
    50000.00,
    0,
    1,
    'Admin',
    decode(replace(gen_random_uuid()::text, '-', ''), 'hex'),
    NOW(),
    NOW()
)
ON CONFLICT ("Document") DO UPDATE SET "Role" = 'Admin';
SELECT "CustomerName", "Document", "Role", "Balance" FROM "Accounts" WHERE "Document" = '44222985007';
