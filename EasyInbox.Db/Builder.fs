namespace EasyInbox.Persistence

open LinqToDB.Configuration

type MySettings() = 
    interface ILinqToDBSettings with
        member this.DataProviders = Seq.empty<IDataProviderSettings>
        member this.DefaultConfiguration = "PostgreSQL"
        member this.DefaultDataProvider = "PostgreSQL"
        member this.ConnectionStrings = 
            seq {
                yield { new IConnectionStringSettings with 
                           member this.Name = "EasyInbox"
                           member this.ProviderName = "PostgreSQL"
                           member this.ConnectionString = "Server = 127.0.0.1; Port = 5432; Database = easyinbox; User Id = postgres; Password = admin"
                           member this.IsGlobal = false }

            } 

module Builder = 
    open LinqToDB.Data

    let setSettings () = 
        DataConnection.DefaultSettings <- MySettings()

        
