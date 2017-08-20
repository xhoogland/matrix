<?php
include('config.php');
include('classes.php');

if (!$debugMode) {
	chdir($cronDir);
	file_put_contents($packedFile, file_get_contents($openDataUrl));
	
	$bufferSize = 1024;
	$inXmlGzFile = gzopen($packedFile, 'rb');
	$outXmlFile = fopen($xmlFile, 'wb'); 

	while (!gzeof($inXmlGzFile)) {
		fwrite($outXmlFile, gzread($inXmlGzFile, $bufferSize));
	}

	fclose($outXmlFile);
	gzclose($inXmlGzFile);
	$outXmlFile = $inXmlGzFile = null;
}

if (!file_exists ($jsonMatrixPortaalLocatiesJsonFile)) {
	die();
}

function getImageNameForSignShown ($display)
{
	$imageName = 'onbekend';
	$tag = null;
	// built in flashers...
	if (isset ($display->restriction_end))
	{
		$tag = $display->restriction_end;
		$imageName = 'einderestrictie';
	}
	elseif (isset ($display->lane_open))
	{
		$tag = $display->lane_open;
		$imageName = 'groenepijl';
	}
	elseif (isset ($display->blank))
	{
		$tag = $display->blank;	
		$imageName = 'leeg';
	}
	elseif (isset ($display->lane_closed))
	{
		$tag = $display->lane_closed;
		$imageName = 'roodkruis';
	}
	elseif (isset ($display->speedlimit))
	{
		$tag = $display->speedlimit;
		$imageName = 'snelheid_' . $display->speedlimit;
	}
	elseif (isset ($display->lane_closed_ahead))
	{
		$tag = $display->lane_closed_ahead;
		$imageName = 'verdrijfpijl_';
		if (isset ($display->lane_closed_ahead->merge_left))
		{
			$imageName = $imageName . 'links';
		}
		else
		{
			$imageName = $imageName . 'rechts';
		}
	}
	
//	return $imageName . '.png';
	return $imageName;
}

$sLiveXmlContents = file_get_contents($xmlFile);
$oLiveMatrixData = simplexml_load_string(str_ireplace('ndw:', '', str_ireplace('soap:', '', $sLiveXmlContents)));
$events = $oLiveMatrixData->Body->NdwVms->variable_message_sign_events->event;

$matrixBorden = [];
foreach ($events as $matrixBord) {
	if (!isset($matrixBord->display))
		continue;

	$vmsInfo = new VmsInfo();
	$vmsInfo->uuid = (string)$matrixBord->sign_id->uuid;
	$vmsInfo->shownSign = getImageNameForSignShown($matrixBord->display);
	
	$matrixBorden[] = $vmsInfo;
}
$sLiveXmlContents = $oLiveMatrixData = $events = null;

file_put_contents($jsonMatrixBordenOutputPath, json_encode($matrixBorden));
die();
