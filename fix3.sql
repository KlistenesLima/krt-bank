ALTER TABLE "PixKeys" ALTER COLUMN "KeyType" TYPE integer USING CASE
    WHEN "KeyType" = 'Cpf' THEN 1
    WHEN "KeyType" = 'Email' THEN 2
    WHEN "KeyType" = 'Phone' THEN 3
    WHEN "KeyType" = 'Random' THEN 4
END;
