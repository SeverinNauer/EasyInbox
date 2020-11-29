namespace EasyInbox.CQService

module Storage = 

    type AddableStorage =
        | GDriveStorage


    type AddStorage = AddableStorage -> Result<string,string>
