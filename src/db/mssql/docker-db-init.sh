#wait for the SQL Server to come up
# sleep 30
echo "Waiting for SQL Server to start..."
for i in {1..50}; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT 1" > /dev/null
    if [ $? -eq 0 ]; then
        echo "SQL Server is ready!"
        break
    else
        echo "SQL Server not ready yet. Attempt $i of 50..."
        sleep 1
    fi
done

if [ $i -eq 50 ]; then
    echo "SQL Server did not start within the expected time. Exiting."
    exit 1
fi

# Remove existing database files
echo "Checking for existing database files..."
DB_FILE_PATH="/var/opt/mssql/data/EventDriveDB.mdf"
LOG_FILE_PATH="/var/opt/mssql/data/EventDriveDB_log.ldf"

if [ -f "$DB_FILE_PATH" ]; then
    echo "Deleting $DB_FILE_PATH..."
    rm -f "$DB_FILE_PATH"
fi

if [ -f "$LOG_FILE_PATH" ]; then
    echo "Deleting $LOG_FILE_PATH..."
    rm -f "$LOG_FILE_PATH"
fi

# Run initialization scripts
echo "Running database initialization script..."
if [ -f /db-init.sql ]; then
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Str0ngP@ssw0rd123 -C -i /db-init.sql
    if [ $? -eq 0 ]; then
        echo "Database initialization script executed successfully."
    else
        echo "Failed to execute the database initialization script."
        exit 1
    fi
else
    echo "No database initialization script found. Skipping."
fi

# Keep the container running
wait

#previously only this:

# echo "running set up script"
# #run the setup script to create the DB and the schema in the DB
# for i in {1..50};
# do
	# /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Str0ngP@ssw0rd123 -C -d master -i db-init.sql
	# if [ $? -eq 0 ]
    # then
        # echo "setup.sql completed"
        # break
    # else
        # echo "not ready yet..."
        # sleep 1
    # fi
# done