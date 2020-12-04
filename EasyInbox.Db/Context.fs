[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Persistence.Context

type Connection() = 
    inherit LinqToDB.Data.DataConnection("EasyInbox")
