module Context

type Connection() = 
    inherit LinqToDB.Data.DataConnection("EasyInbox")
