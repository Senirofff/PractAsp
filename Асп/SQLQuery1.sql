ALTER TABLE Users ADD discount_percent INT NOT NULL DEFAULT 0;
ALTER TABLE Users ADD is_active BIT NOT NULL DEFAULT 1;
UPDATE Users 
SET discount_percent = 15 
WHERE id_role = 4;