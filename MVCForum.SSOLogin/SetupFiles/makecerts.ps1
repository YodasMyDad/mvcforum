$issuerCertificate = "localhost"
$tokenCertificates = "TokenSigningCert", "TokenEncryptingCert"   
   
$makecert = 'C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\makecert.exe'
$certmgr = 'C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\certmgr.exe'
   
function CreateIssuerCertificate {
	param($certificateSubjectName)
    $exists= ls cert:\LocalMachine\My | select subject | select-string "cn=$certificateSubjectName"
    if($exists -ne $null)
    {
        echo "$certificateSubjectName certificate already exists"
    }
    else
    {
        ls $env:temp\$certificateSubjectName.* | del
        & $makecert -r -pe -n "cn=$certificateSubjectName" -ss My -sr LocalMachine -sky exchange -sy 12 "$env:temp\$certificateSubjectName.cer"
        & $certmgr -add -c "$env:temp\$certificateSubjectName.cer" -s -r localmachine root
    }
  }   
function CreateTokenCertificate {
	param($certificateSubjectName, $issuerCertificateSubjectName)
	$exists= ls cert:\LocalMachine\TrustedPeople | select subject | select-string "cn=$certificateSubjectName"
	if($exists -ne $null)
	{
		echo "$certificateSubjectName certificate already exists"
	}
	else
	{
		& $makecert -pe -n "cn=$certificateSubjectName" -ss TrustedPeople -sr LocalMachine -sky exchange -sy 12 -in "$issuerCertificateSubjectName" -ir LocalMachine -is TrustedPeople "$env:temp\$certificateSubjectName.cer"
	}
}
     
     
     
CreateIssuerCertificate $issuerCertificate
     
foreach($cert in $tokenCertificates)
{
	write-host "Creating certificate $cert (signed by $issuerCertificate)"
	CreateTokenCertificate $cert "$issuerCertificate"
}