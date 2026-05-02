# Portable POS Database Folder
=============================

This folder (`/db-data`) contains the user database for your POS System.
Because you are running on macOS using Docker Azure SQL Edge, the application requires the system databases (`master`, `model`, etc.) to be hosted on virtiofs-compatible volumes to avoid core dump crashes. 

To keep this folder clean without breaking SQL Server:
1. System databases are safely mapped to a hidden folder: `.system-db/`
2. This folder (`db-data/`) contains exclusively `POS_DB.mdf` and `POS_DB_log.ldf`.

Your connection string targets `Database=POS_DB`, and SQL Server attached this directly.

## To Backup your Data:
Just copy the `POS_DB.mdf` and `POS_DB_log.ldf` out of this folder!
