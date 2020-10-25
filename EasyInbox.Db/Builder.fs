namespace EasyInbox.Persistence

open LinqToDB.Configuration

type Settings() = 
    interface IConnectionStringSettings with
       member this.Name = "EasyInbox"
       member this.ProviderName = "PostgreSQL"
       member this.ConnectionString = "Server = 127.0.0.1; Port = 5432; Database = easyinbox; User Id = postgres; Password = admin"
       member this.IsGlobal = false


type MySettings() = 
    interface ILinqToDBSettings with
        member this.DataProviders = Seq.empty<IDataProviderSettings>
        member this.DefaultConfiguration = "PostgreSQL"
        member this.DefaultDataProvider = "PostgreSQL"
        member this.ConnectionStrings = 
            seq {
                yield Settings() :> IConnectionStringSettings    
            } 

module Builder = 
    open LinqToDB.Data

    let setSettings () = 
        DataConnection.DefaultSettings <- MySettings() :> ILinqToDBSettings

        
