class Test {
	public static function main()  {
		trace("Hello world");
	}
}

class Table {
	var name : String;
	var rows : Array<Row>;
}

class Row {
	var items : Array<SqlValue>;
}

class Column {
	var name : String;
	var type : SqlType;
}

enum SqlValue {
	Bit( x : Bool );
	Int( x : haxe.Int32 );
}

enum SqlType {
	Bit;
	DateTime;
	Varchar( size : Int );
	NVarchar( size : Int );
	Int;
	BigInt;
}
