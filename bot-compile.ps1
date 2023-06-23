param(
	[string]
	$TargetName="ktwrd/xenia-revolt",
	[string]
	$TargetTag="latest",
	[bool]
	$IncludeTimeTag=1
)
$targetTag="$($TargetName):$($TargetTag)"
docker build -t xenia-revolt:latest .

docker tag xenia-revolt:latest $targetTag
docker push $targetTag

if ($IncludeTimeTag -eq $True)
{
	$currentTimeTag=@(Get-Date -UFormat %s -Millisecond 0)
	$timeTag="$($TargetName):$($currentTimeTag)"
	docker tag xenia-revolt:latest $timeTag
	docker push $timeTag
}