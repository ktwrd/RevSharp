param(
	[string]
	$TargetName="ktwrd/skidbot-revolt",
	[string]
	$TargetTag="latest",
	[bool]
	$IncludeTimeTag=1
)
$targetTag="$($TargetName):$($TargetTag)"
docker build -t skidbot-revolt:latest .

docker tag skidbot-revolt:latest $targetTag
docker push $targetTag

if ($IncludeTimeTag -eq $True)
{
	$currentTimeTag=@(Get-Date -UFormat %s -Millisecond 0)
	$timeTag="$($TargetName):$($currentTimeTag)"
	docker tag skidbot-revolt:latest $timeTag
	docker push $timeTag
}