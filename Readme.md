# Bastet - A Home Automation Server In C\#

Bastet is a server which acts as a proxy between [CoAP](https://datatracker.ietf.org/doc/draft-ietf-core-coap/) devices spread around the home and control scripts running on a server talking HTTP.

Bastet provides:

 - CoAP <-> HTTP translation
 - Data Logging
 - Authentication

## Quick Start

To start a Bastet server you just need to start the application with two commandline parameters:

    --http 58242 --connection="Data Source=Bastet.sqlite;Version=3;New=True;" --setup -lni
    
This will start an interactive setup to configure Bastet. You can now visit [http://localhost:58241](http://localhost:58241) to see the Bastet status screen.

#### http

This is the port the http server will bind to

#### connection

This is the database connection string, the example is a sqlite database on disk called *Bastet.sqlite*.

#### Setup

This indicates that the node should be "setup" cleanly, i.e. tables will be created, and a new user/password will be inserted. The username will be "Administrator" and password will be "password".

#### l, n and i

These flags indicate which addresses should be bound by the HTTP server.

-l will bind the "localhost" address

-n will bind the netBIOS machine name address

-i will bind all non localhost IPs

## API

The main purpose of Bastet is to act as a proxy between your control scripts, which talk to Bastet using HTTP, and your home automation devices which talk to Bastet using CoAP. The root of the API is at [http://localhost:58241/api](http://localhost:58241/api).

 - [Authentication](#authentication)
 - [Users](#users)
 - [Claims](#claims)
 - Devices
 - Devices (Proxying)
 - Devices (Sensors)

### Authentication

Almost all of the API requires authentication to use, so this is the first thing to check out! There are two ways to authenticate your requests:

 - A session cookie
 - A session key in the query string
 
To get your session key POST to /authentication. You can send your parameters in one of three ways:

 - [HTTP basic auth](https://en.wikipedia.org/wiki/Basic_access_authentication)
 - "username" and "password" parameters in query string
 - "username" and "password" fields in form data
 
This will return a your session key, as well as set a cookie with your session key. From here on you can either send the cookie along with your requests, or include _sessionkey=thisismysupersecretsessionkey_ in your query string. To logout simply DELETE to /authentication and this session key will no longer be valid.

### Claims

User permissions are managed with "claims". Certain actions require a user to have a certain claim to perform them, for example getting a list of all users (GET /users) requires the "list-users" claim.

#### Get Claims For Specific User

 - GET /users/{username}/claims
 - Requires claims: "list-claims"
 
#### Create A Claim For A Specific User

 - POST /users/{username}/claims
 - Requires claims: "create-claim"
 - Body should be the name of the claim
 
#### Delete A Claim For A Specific User

 - DELETE /users/{username}/claims
 - Requires claims: "delete-claim"
 - Body should be the name of the claim

### Users

#### List All Users

 - GET /users
 - Requires claims: "list-users"
 
#### Get Specific User

 - GET /users/{username}
 
#### Create User
 
  - POST /user
  - Send "username" and "password" in Query string or form data








