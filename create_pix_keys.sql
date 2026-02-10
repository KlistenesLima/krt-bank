CREATE TABLE pix_keys (
    id UUID PRIMARY KEY,
    account_id UUID NOT NULL,
    key_type VARCHAR(10) NOT NULL,
    key_value VARCHAR(77) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deactivated_at TIMESTAMP NULL,
    CONSTRAINT fk_pix_keys_account FOREIGN KEY (account_id) REFERENCES "Accounts" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX ix_pix_keys_type_value ON pix_keys (key_type, key_value) WHERE is_active = true;
CREATE INDEX ix_pix_keys_account_id ON pix_keys (account_id);
