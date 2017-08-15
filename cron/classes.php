<?php
class RoadLocation {
	public $coordinate; // type: Coordinate
	
	public $matrixPortal; // type: MatrixPortal
}

class Coordinate {
	public $lat; // type: float
	
	public $lon; // type: float
}

class MatrixPortal {
	public $roadWays; // type: array (of RoadWay)
}

class RoadWay {
	public $hmLocation; // type: string
	
	public $lanes; // type: array (of Lane)
}

class Lane {
	public $uuid; // type: string
	
	public $number; // type: int
	
	public $shownSign; // type: string
}
