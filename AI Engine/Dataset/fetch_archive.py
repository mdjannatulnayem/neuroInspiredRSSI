import pyodbc, pandas as pd
from datetime import datetime
import os

# Prompt for database connection details
server = input("Enter the server name (e.g., your_server.database.windows.net): ")
database = input("Enter the database name: ")
username = input("Enter the username: ")
password = input("Enter the password: ")
table_name = input("Enter the table name: ")

# Create 'Archive' folder if it doesn't exist
archive_folder = 'Archive'
os.makedirs(archive_folder, exist_ok=True)

# Create a timestamped filename
timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
output_csv_file = os.path.join(archive_folder, f'{table_name}_{timestamp}.csv')

# Establish the database connection
conn_str = f'DRIVER={{ODBC Driver 17 for SQL Server}};SERVER={server};DATABASE={database};UID={username};PWD={password}'

# Connect to the database
try:
    conn = pyodbc.connect(conn_str)
    print("Connected to the database successfully.")

    # Read the table into a DataFrame
    query = f"SELECT * FROM {table_name}"
    df = pd.read_sql(query, conn)

    # Save DataFrame to a timestamped CSV file in the Archive folder
    df.to_csv(output_csv_file, index=False)
    print(f"Data saved to {output_csv_file} successfully.")
    
except pyodbc.Error as e:
    print(f"Error: {e}")
finally:
    # Close the connection
    conn.close()
