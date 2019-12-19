[![Build Status](https://travis-ci.org/rkoeninger/PocketSql.svg?branch=master)](https://travis-ci.org/rkoeninger/PocketSql)
[![Latest Nuget](https://img.shields.io/nuget/v/PocketSql.svg)](https://www.nuget.org/packages/PocketSql)

# PocketSql

An in-process implementation of T-SQL, packaged as a library, suitable for testing data acess code and queries.

### Planned/Implemented Features

  * `create`, `alter`, `drop` for
    * Tables
	* Views
	* Procedures
	* Functions
	  * Scalar
	  * Table-valued
  * `select`, `update`, `insert`, `delete`, `truncate`, `merge` with
    * `output` for DML statements
	* `offset ... fetch` for queries
	* `select ... into`
	* `insert ... select`
	* `update ... join`
	* `distinct`
  * CTEs (Common Table Expressions)
  * `if`, `while` statments
  * `case`, `between` expressions
  * All binary operators
  * `sum`, `count`, `rownumber` aggregate functions
  * Calling user-defined functions
  * Input, output and return parameters
  * `declare`, `set`, access local variables
  * `@@rowcount`, `@@fetch_status`, etc. meta-variables
  * Cursors
  * Uniqueness constraints imposed by indexes
  * `identity` auto-incrementing columns
    * constraints imposed by it
	* ability to set indentity insert on and off
  * `default` values for columns
  * implicit type conversions and type checking

### Currently Unplanned/Excluded Features

  * `waitfor`
  * `break`, `continue`, `goto`, labels
  * Working Transactions with `rollback`/`commit`
  * Nested table results
  * Customizable collations (everything is case insensitive by default)
  * Backup/restore capability
  * `INFORMATION_SCHEMA` and other control tables and procedures
  * Optimizations around indexes
  * `for` keyword that specifies output format
  * XML types and namespaces
  * any special handling of the `go` statement
  * Query plan caching
  * Ability to report query plan
