SELECT 'CREATE DATABASE "MiKompri_ProductCatalog"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'MiKompri_ProductCatalog')\gexec
