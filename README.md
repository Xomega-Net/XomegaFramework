# Xomega Framework 

A powerful .NET framework for building multi-tier data driven web and desktop applications backed by a service layer based on Entity Framework or any other ORM framework.

The framework has been built from more than 15 years of experience, and is based on the approach that has been proven in many large scale applications. The primary goals of the framework are to:

- Enable rapid application development.
- Promote reusability, which can significantly reduce development and maintenance costs.
- Ensure consistency to deliver the best user experience for the application.

## Components

The framework consists of the core package, as well as several additional packages that target specific technologies, as follows.

- **Xomega.Framework** - the core package that contains reusable code for both web and desktop clients, as well as for the service layer.
- **Xomega.Framework.Web** - implementation of views and data bindings for ASP.NET WebForms.
- **Xomega.Framework.Wpf** - implementation of views and data bindings for WPF.
- **Xomega.Framework.Wcf** - support for hosting and using business services in WCF.

## Features

Xomega framework provides rich base and utility classes for building presentation layer data objects that can be bound to the standard ASP.NET or WPF controls, and then used to build view models and views from those. Some of the important features that the framework supports are as follows.

- **Data Properties** encapsulate a data value (or multiple values) and metadata, such as editability, visibility, security access, required flag, possible values, modification tracking, and also formatting, validation and conversion rules for the values. Properties can notify listeners about changes in the value or the metadata, and can be bound to UI controls, which would reflect any changes in the data properties, and update the property value from the user input.

- **Data Objects** contain data properties and nested child data objects, as well as its own overarching metadata, such as editability, security access level, modification tracking, and object-level validation rules. They also provide some CRUD support, and the ability to export/import its data to/from the service data contracts.

- **Data List Objects** are special type of data objects that are optimized for working with tabular data. They provide additional list-related functionality, including notifications about collection changes, support for multi-property sorting and row selection, and tracking of the applied criteria.

- **Caching of reference data** on the client allows you to quickly populate selection lists, and to look up a data item by a unique key using self-indexing look-up tables. It supports multiple types of caches with extensible cache loaders, and ability to store additional attributes with each item.

- **View Models and Views** provide platform-independent base classes for presentation views and their view models. They support publishing of view-level events, such as Close, Save or Delete, as well as navigation between views with input and output parameters, and handling updates from the child views.

- **Search Views** implement support for search screens with a results grid that may allow selection, and a criteria section with flexible search operators. This includes both the generic logic, and the specific implementations for ASP.NET and WPF. 

- **Details Views** implement CRUD logic, validation and modification tracking for details screens both generically, and specifically for ASP.NET and WPF.

- **Bindings for common ASP.NET and WPF controls** to Data Properties and Data Objects allows you to easily attach views to the view models and their underlying data objects.

- **Client-Side Validation** takes care of running all standard and custom validations on your data objects, highlighting invalid fields with an error tooltip, and also displaying a list of all validation errors using proper field labels in the error text.

- **Error Handling** provides an extensible framework for reporting errors both on the client side, and from the server side with internationalization, as needed.

- **Dependency Injection** powers the flexibility of the framework, allowing you to provide custom implementations for various features, and to build generic objects that can be reused in different contexts.
 
## Getting Started

- The easiest way to get started with Xomega Framework is to install our free Visual Studio extension Xomega.Net, which provides preconfigured project templates for ASP.NET, WPF and SPA applications, and allows you to instantly generate complete and powerful search and details views right from your service model. Please check [our web site](http://xomega.net) for more details.

- A more manual way involves adding Xomega NuGet packages to your Visual Studio project, and reading [How To guides on our forum](http://xomega.net/Tutorials/HowTos.aspx). Feel free to post your questions there if you don't find the information you need.

- And, of course, you can always download the code and build it manually. You're welcome to post any issues, fork the code and create pull requests with any fixes or enhancements.
 
## Additional Resources

Below are some tutorials and articles that can help you ramp up with the framework.

### Tutorials

- [Complete Walkthrough](http://xomega.net/Tutorials/WalkThrough.aspx) - a comprehensive step-by-step guide to Model-Driven Development with Xomega.Net and Xomega Framework, which walks you through building full fledged web and desktop applications with Xomega, demonstrating common use cases along the way.

- [How-To Guides](http://xomega.net/Tutorials/HowTos.aspx) - individual step-by-step guides that cover various topics related to developing applications with Xomega Framework and Xomega.Net.

### Articles

- [Take MVC to the Next Level in .Net](http://www.codeproject.com/KB/WPF/xomfwk.aspx) - in-depth run through framework's UI features with examples.
- [How to Build Powerful Search Forms](http://www.codeproject.com/KB/usability/PowerSearch.aspx) - demonstrates application of the framework for building advanced search forms.
- [Cascading Selection the MVC Way](http://www.codeproject.com/Articles/545906/Cascading-Selection-the-MVC-Way) - describes working with cached static data, selection controls, cascading selection, etc.
- [How to Build Flexible and Reusable WCF Services](http://www.codeproject.com/Articles/317232/How-to-Build-Flexible-and-Reusable-WCF-Services) - framework's support for service layer best practices and design patterns.
