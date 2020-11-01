# Pgadmin
Pgadmin is a webinterface.

# Login
You can login with the credentials in the file .env in this directory.

# Add postgres datasource
You can add a server via Servers (on the left) -> create -> Server..

The connection data (under tag connection) for the docker-compose postgres database are:

    Host name: postgres
    Port: 5432
    Maintenance database: easyinbox
    Username: easyinbox
    Password: easyinbox
    Role:
    Service:

This data may change if you have changed the docker-compose or any .env file.