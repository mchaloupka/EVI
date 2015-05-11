$sqlInstance = "(local)\SQL2014";
$dbName = "R2RMLTestStore";

function Update-Config 
{
	$mssqlConnectionString = "Server=$sqlInstance;Database=$dbName;User ID=sa;Password=Password12!";
	
	foreach($o in $input) 
	{
		Write-output ('Updating ' + $o.FullName)
	
		$doc = (Get-Content $o.FullName) -as [xml];
		$node = $doc.SelectSingleNode('//connectionStrings/add[@name="mssql_connection"]');
		
		if($node) 
		{
			Write-output ('Updating MSSQL ConnectionString to: ' + $mssqlConnectionString);
			$node.connectionString = $mssqlConnectionString;	
		}
        else
        {
            Write-output ('No MSSQL config found');
			Push-AppveyorArtifact $o.FullName
        }
		
		$doc.Save($o.FullName);
	}
	
	
}

$env:APPVEYOR_BUILD_FOLDER | get-childitem -recurse |? {$_.Name -eq "Slp.r2rml4net.Test.System.dll.config"} | Update-Config;

Write-output ('Creating database ' + $dbName);
sqlcmd -S "$sqlInstance" -Q "Use [master]; CREATE DATABASE [$dbName]";
