# Bastet - A Home Automation Server In C#

Bastet is a server which acts as a proxy between [CoAP](https://datatracker.ietf.org/doc/draft-ietf-core-coap/) devices spread around the home and control scripts running on a server talking HTTP.

Bastet provides:

 - CoAP <-> HTTP translation
 - Data Logging
 - Authentication

## Quick Start

To start a Bastet server you just need to start the application with two commandline parameters:

    --setup true --http 58241
    
This will start an interactive setup to configure Bastet. You can now visit [http://localhost:58241](http://localhost:58241) to see the Bastet status screen.

## API

The main purpose of Bastet is to act as a proxy between your control scripts, which talk to Bastet using HTTP, and your home automation devices which talk to Bastet using CoAP. The root of the API is at [http://localhost:58241/api](http://localhost:58241/api).

 - [Authentication](#authentication)
 - [Users](#users)
 - [Claims](#claims)
 - Devices
 - Devices (Proxying)
 - Devices (Sensors)

#### Authentication

Almost all of the API requires authentication to use, so this is the first thing to check out! There are two ways to authenticate your requests:

 - A session cookie
 - A session key in the query string
 
To get your session key POST to /authentication. You can send your parameters in one of three ways:

 - [HTTP basic auth](https://en.wikipedia.org/wiki/Basic_access_authentication)
 - "username" and "password" parameters in query string
 - "username" and "password" fields in form data
 
This will return a your session key, as well as set a cookie with your session key. From here on you can either send the cookie along with your requests, or include _sessionkey=thisismysupersecretsessionkey_ in your query string. To logout simply DELETE to /authentication and this session key will no longer be valid.

#### Users

Sending a GET request to /users will list all users, if you have permission (requires [claim](#claims) "list-users"). To create a new user POST to /users with "username" and "password" in either the query string or the form data.

#### Claims

User permissions are managed with "claims". Certain actions require a user to have a certain claim to perform them, for example getting a list of all users (GET /users) requires the "list-users" claim.

To get claims for a specific user GET /users/{username}/claims, this requires the "list-claims" claim.

To create a claim for a user POST /users/{username}/claims, this requires the "create-claim" claim. The body of the request should be the name of the claim to create.

Similarly to delete a claim for a user DELETE /users/{username}/claims, this requires the "delete-claim" claims. The body of the request should be the name of the claim to delete.








