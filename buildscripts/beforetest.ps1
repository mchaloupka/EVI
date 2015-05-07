function Update-Config 
{
	$mssqlConnectionString = "Data Source=(local)\SQL2014;Initial Catalog=master;User ID=sa;Password=Password12!";
	
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
		
		$doc.Save($o.FullName);
	}
	
}

$env:APPVEYOR_BUILD_FOLDER | get-childitem -recurse |? {$_.Name -eq "app.config"} | Update-Config;
