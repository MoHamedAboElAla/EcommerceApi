# Ecommerce API

This project is a simple and clean **E-commerce REST API** built using **ASP.NET Core Web API**. It simulates the backend of a basic online store, supporting features like user authentication, product management, shopping cart, and order processing.

The goal of this project is to provide a lightweight, easy-to-understand foundation for anyone who wants to build or learn how an e-commerce backend works.

## Features

- User Registration & Login
- Product Creation
- Shopping Cart (Add/Remove items)
- Order Placement & History
- Role-based behavior (Admin vs Normal User)
- SQL Server Integration
- Clean and simple code structure


### User Authentication

- Users can **register** with basic information like name, email, and password.
- Users can **log in** with email and password to access their account.
- Basic validation is applied to prevent duplicate emails and invalid logins.

### User Management

- Admins can **view all registered users**.
- Admins can **delete any user** by ID.

### Shopping Cart

- Users can **add products to their cart**.
- Users can **view the items** currently in their cart.
- Users can **remove specific items** from the cart.


### Order Handling

- Users can **place an order** based on the items in their cart.
- All items from the cart are included in the order, and the cart is cleared after checkout.
- Users can **view their order history** with order details like date, total price, and items.




