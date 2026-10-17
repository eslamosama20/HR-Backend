$connectionStringMain = "Server=localhost;Database=ErpMain;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"
$connectionStringERP = "Server=localhost;Database=ERP;Integrated Security=True;TrustServerCertificate=True;Encrypt=False"

function Check-Database($connStr, $dbName) {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connStr
    try {
        $connection.Open()
        Write-Host "Connected successfully to $($dbName) database!"
        
        $cmd = $connection.CreateCommand()
        $cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Projects'"
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
        $dt = New-Object System.Data.DataTable
        $adapter.Fill($dt) | Out-Null
        Write-Host "Columns in Projects table of $($dbName):"
        $cols = $dt | Select-Object -ExpandProperty COLUMN_NAME
        $cols -join ", " | Write-Host
    }
    catch {
        Write-Host "Error connecting to $($dbName) - $_"
    }
    finally {
        $connection.Close()
    }
}

Write-Host "Checking ErpMain:"
Check-Database $connectionStringMain "ErpMain"
Write-Host "`nChecking ERP:"
Check-Database $connectionStringERP "ERP"
