# Multiplayer hangman api
A multiplayer hangman rest-api written in C# with .NET

## 🔧 Installing
Requierments:
- Docker
- MySql / Mariadb Database

You can pull the docker image using:
```
  docker pull ghcr.io/larvenstein/multiplayer-hangman-api:latest
```

If you want to use docker-run, you can use this:
```
docker run --env=Server=192.168.1.100 --env=User=myusername --env=Password=mypassword --env=Database=mydatabase -p 8080:8080 -p 8081:8081 ghcr.io/larvenstein/multiplayer-hangman-api:latest
```

I reccomend to use docker-compose, here is a template with a mariadb database:
[compose.yaml](https://raw.githubusercontent.com/LarvenStein/multiplayer-hangman-api/main/compose.yaml)
```yaml
version: '3.8'

services:
  mariadb:
    image: mariadb
    environment:
      MYSQL_ROOT_PASSWORD: examplepassword
      MYSQL_DATABASE: hangmandb
      MYSQL_USER: root
      MYSQL_PASSWORD: examplepassword
    ports:
      - '3306:3306'
    healthcheck:
      test: ["CMD", "healthcheck.sh", "--connect", "--innodb_initialized"]
      start_period: 10s
      interval: 10s
      timeout: 5s
      retries: 3

  multiplayer-hangman-api:
    image: ghcr.io/larvenstein/multiplayer-hangman-api:latest
    environment:
      Server: mariadb
      User: root
      Password: examplepassword
      Database: hangmandb
    ports:
      - "8080:8080"
      - "8081:8081"
    depends_on:
      mariadb:
        condition: service_healthy
    links:
      - mariadb
```

If  you want to, you can use my 6 wordlists with ~ 2 million words by connecting to the database with a client and then importing [this sql file](http://example.com)

## 🕹️ Playing
As this is a rest api, you can play this with any client you want like:
- My own frontend written in svelte
  - [Play it](http://example.com)
  - [GitHub repo](https://github.com/LarvenStein/multiplayer-hangman-frontend)
- Using a tool like insomnia or postman
- Using a client that you developed yourself

## 📞 Endpoints
If you want to want to develop your own client or just play around with the api, here is a list of all the endpoints and what they do:

| **Endpoint**                                                 | **Description**                                                   | **Parameter**                                                                                                                | **Response**                                                                                                                                                                                 |
|--------------------------------------------------------------|-------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **POST**<br> `/api/games`                                    | Creates a new game and returns the roomCode                       | -                                                                                                                            | `{"roomCode": "string"}`                                                                                                                                                                     |
| **POST**<br> `/api/games/{roomCode}/players`                 | Puts user in specified room                                       | **Body**<br> `{ "nickname": "string"}`                                                                                       | ` {"id": "guid", "nickname": "string", "roomCode": "string"}`                                                                                                                                |
| **PUT**<br> `/api/games/{roomCode}`                          | Edits game settings                                               | **Body**<br> `{"rounds":int, "maxPlayers":int, "newGameLeader":"guid", "wordList":int}`<br> **Header**<br> `x-user-id: guid` | `{"rounds":int, "maxPlayers":int, "wordList":int}`                                                                                                                                           |
| **POST**<br> `/api/games/{roomCode}`                         | Starts the game                                                   | **Header**<br> `x-user-id: guid`                                                                                             | `{"roundId": int}`                                                                                                                                                                           |
| **GET**<br> `/api/games/{roomCode}`                          | Returns information about the room                                | **Header**<br> `x-user-id: guid`                                                                                             | `{"roomCode": "string", "maxPlayers": int, "rounds": int, "wordList": "string", "status": "string", "round": int}`                                                                           |
| **GET**<br> `/api/wordlists`                                 | Returns available wordlists                                       | -                                                                                                                            | `[{"id": int, "name": "string"}, {"id": int, "name": "string"}]`                                                                                                                             |
| **GET**<br> `/api/{roomCode}/players`                        | Returns all players in room and  if current player is game leader | **Header**<br> `x-user-id: guid`                                                                                             | `{"players": ["string", "string"], "isPlayerGameLeader": bool}`                                                                                                                              |
| **GET**<br> `/api/games/{roomCode}/rounds/{roundNum}`        | Returns information about the specified round                     | **Header**<br> `x-user-id: guid`                                                                                             | `{"roomCode":"string", "roundNum":int, "status":"string", "correctGuesses":int, "falseGuesses":int, "lifesLeft":int, "guessedWord":["char", "char"], "wrongLetters":["string", "string"]}`   |
| **GET**<br> `/api/games/{roomCode}/rounds`                   | Returns information about all rounds in room                      | **Header**<br> `x-user-id: guid`                                                                                             | `[{"roomCode":"string", "roundNum":int, "status":"string", "correctGuesses":int, "falseGuesses":int, "lifesLeft":int, "guessedWord":["char", "char"], "wrongLetters":["string", "string"]}]` |
| **POST**<br> `/api/games/{roomCode}/rounds/{roundNum}/guess` | Submits a guess                                                   | **Body**<br> `{"guess": string}`<br> **Header**<br> `x-user-id: guid`                                                        | `{"guess": "string", "correct": bool, "roundNum": int}`                                                                                                                                      |

I also made an insomnia collection with post request scripts and env vars, that you don't have to do everything by hand:

[Insomnia collection](https://github.com/user-attachments/files/16066818/hangman-api_collection.json)
