# Docker & docker-compose
This readme is all about how you have to spin up the database for EasyInbox.

## Services
- [Postgresql](./postgres/README.md)
- [pgadmin](./pgadmin/README.md)

## Install docker-compose
If you dont have docker-compose allready installed please install it. Docker-compose doesnt come with docker installation but it relies on the docker engine.
The installation guide for docker-compose can be found [here](https://docs.docker.com/compose/install/).
Please remember that if you are on Windows docker compose comes with "Docker Desktop for Windows" and you most likely dont have to install docker-compose separately.

## Create volume
You have to create the volume first to persist data. The command to create the volume is:

    docker volume create --name=<volumename>

If you want to remove the persist data execute this command:

    docker volume rm <volumename>

But remember to create the volume again if you want to start the postgres service again.
The volumes to create are in the docker-compose file under volumes. 
If the volume does not exist, docker-compose will not run.

## Run services
To run docker-compose just execute this command:

    docker-compose up -d

Use "-d" if you  want that it's running as deamon.

If you want to let run only one service just run this command:

    docker-compose up -d <servicename>

## See logs
To see the logs of the docker-compose file execute this command:

    docker-compose logs

To see logs from one service only:

    docker-compose logs <servicename>

If you want to follow the logs you can add "-d" like this

    docker-compose logs <servicename or not> -f

## Shutdown
To shut down all services in the docker-compose file:

    docker-compose down

Or if you want to shut down only one service:

    docker-compose down <servicename>

## Environment file
docker-compose will pick up the ".env" file if it exists.