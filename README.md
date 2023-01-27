# Web Engineering Core Project: Back-end
This is the back-end API for my Web Engineering course's main project. It was made using .NET Core, besides the fact that I find this framework easy to use and quite flexible; it is also ideal for implementing services such as authorization, which was essential for some of the API's features. The biggest challenge to consider if an API like this is to be replicated is, I think, the implementations of CORS and JWT for authorization. CORS is essential to consume an API, understanding it was not particularly difficult but finding a way to add it to the API's services took some research. When it comes to JWT and authorization, it is worth it to understand how to use the token in the HTTP headers and properly use encryption considering the specifics of the algorithm used for cryptography.

# Installing and running the project locally

Since it is a .NET Framework app, you should ideally run it on Visual Studio once you have downloaded the code so that it runs locally and you can test it, for this don't forget to add the ***ASP.NET and web development*** workload. If you prefer, you can use Visual Studio Code, it is lighter and it will work as well if you run it with "dotnet run" in the terminal. Remember, this is a .NET API so if you use VS Code you also need the .NET SDK. To consume the API, it is also quite important to properly set up CORS, meaning you have to add the allowed origins for the front-end.

# Deployed API

This API is deployed in Heroku, you can find it here (but you need to use the right URLs, the following won't really work):
  -  [https://ingweb-back-hiriart.herokuapp.com/](https://ingweb-back-hiriart.herokuapp.com/)

To see this API at work without running it locally or deploying it yourself, either check out my front-end here on [GitHub](https://github.com/Diego-Hiriart/IngWeb-Frontend-DiegoHiriart) or on [Heroku](https://ingweb-front-hiriart.herokuapp.com/), or visit these URLs to see some the JSONs available with an authorization-less GET method:
  - [https://ingweb-back-hiriart.herokuapp.com/api/brands](https://ingweb-back-hiriart.herokuapp.com/api/brands)
  - [https://ingweb-back-hiriart.herokuapp.com/api/models/get-all](https://ingweb-back-hiriart.herokuapp.com/api/models/get-all)
  - [https://ingweb-back-hiriart.herokuapp.com/api/posts](https://ingweb-back-hiriart.herokuapp.com/api/posts)

# Using the API

Before you use the API, you must make sure you:
  - Set up adequate CORS policies for the front-end.
  - Create a valid a user so that you can obtain the authorization token, for that, you must use the ***Login*** method of the [Authorization Controller](https://github.com/Diego-Hiriart/IngWeb-Backend-DiegoHiriart/blob/main/Controllers/AuthorizationController.cs).

There are essentially two types of endpoints you can use:
 - Endpoints that need authorization, for these ones you must send a token in the HTTP request's authorization header with the "bearer \[token\]" format. Some endpoints need you to have and admin role, you can check which ones need this in the corresponding [controllers](https://github.com/Diego-Hiriart/IngWeb-Backend-DiegoHiriart/tree/main/Controllers).
 - Authorization-less endpoints, which you can just use without sending a token in the authorization header.

# Core functionality of the project
This project essentially consists of a web site to post reviews of tech products in general. The purpose of this project is to offer a repository that users can go to when they need to research a product's potential issues and how long it will last. A site like this would be especially useful when looking to buy a new tech product, since ideally you want something that lasts and isn't difficult to maintain. Because of this, the web site is not limited to plain text or score-based reviews and allows users to add the product's life span and issues when making a review. The information posted by users when reviewing can then be used by others to look up which products are best suited for their needs.

As mentioned, users will be able to post reviews with details regarding the life span their specific instance of the product had, how long it lasted without presenting issues, and the issues themselves (these are problems with the product, e.g., a faulty RAM stick). These posts feed the repository for the web site and are the main source of information for people doing research about a specific model or looking for products that match ceratin filters. When users are researching products, they can either view durability and issue data for a specific model, or search for models that meet criteria for life span, time without issues, number of reviews, percentage of issues with reviews, and percentage of those issues that can be fixed.

To ensure that there aren´t several copies of the same product, brand, or component, users can only choose from a list of these, which have been previously defined (and must be kept updated) by the administrators. Mainly, this avoids situations in which products are called differently by different people, but from a model number perspective they are the same. The administrators' role doesn't extend much further than this, they can use the platform like a regular user besides these and user management (deletion and admin privileges) functionalities.

# Useful resources / Documentation
  - [Enable Cross-Origin Requests (CORS) in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0)
  - [CRUD with a .NET 6 Web API & Entity Framework Core 🚀 Full Course](https://youtu.be/Fbf_ua2t6v4)
  - [.NET 6 Web API 🔒 Create JSON Web Tokens (JWT) - User Registration / Login / Authentication](https://youtu.be/v7q3pEK1EA0)
  - [.NET 6 Web API 🔒 Role-Based Authorization with JSON Web Tokens (JWT)](https://youtu.be/TDY_DtTEkes)

# Contact
[hiriart.leon.d@gmail.com](mailto:hiriart.leon.d@gmail.com)
