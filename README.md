# Xomega Framework

Xomega Framework is a full-stack open-source application framework for quickly building well-architected data-oriented web or desktop apps in .NET.

The framework has been built from more than 20 years of experience and is based on an approach that has been proven in many large-scale enterprise applications. The primary goals of the framework are to:

- ✔️ Enable rapid application development.
- ✔️ Employ future-proof best practice architectures.
- ✔️ Promote reusability to reduce development and maintenance costs.
- ✔️ Ensure consistency to deliver the best user experience for your apps.

## Framework features

Following is a summary of the framework’s major features.
- [x] Framework for **reusable Business Services**, either exposed via remote REST API or integrated into the app.
- [x] Common **error reporting framework**.
- [x] **Lookup reference data** handling and caching.
- [x] **Internationalization** and localization support.
- [x] **Generic MVVM framework** for the presentation data model and logic.
  - [x] Base classes for advanced Search and Details views.
  - [x] Consistent display of data in the proper formats.
  - [x] Automatic client-side validations with error display.
  - [x] Modification tracking with unsaved changes prompts.
- [x] **Views and components** that are bound-able to the generic view models for the following UI frameworks:
  - [x] Blazor Server.
  - [x] Blazor WebAssembly.
  - [x] Syncfusion Blazor components with in-grid editing.
  - [x] WPF views and controls.
  - [x] Legacy ASP.NET WebForms.

## Why do I need it?

If you need to build data-oriented apps that require searching data by a variety of advanced criteria, displaying the results in a tabular grid, and viewing or editing details in separate forms or directly in the grids, then you should definitely consider using Xomega Framework.

### Line-of-Business apps

For example, let's consider that your team is building a large-scale enterprise-grade Line-of-Business application or a number of smaller-scale LOB applications.

If you go with a server-side architecture, such as Blazor Server, your developers may not provide a strict separation between the business services and the presentation logic, as per the best practices. This will make it difficult to expose those services via Web API and switch to a SPA architecture, such as Blazor WebAssembly. Xomega Framework provides a standardized way to implement business services, check security, perform validations, report errors, etc, which allows you to easily switch from one architecture to another.

### Reusability

Without any framework, your developers will have to manually implement displaying data in the proper formats, modification tracking, client-side validations, and other common aspects of the presentation logic directly in each form. Those things will likely be copy-and-pasted from one form to another, thereby substantially increasing maintenance and testing costs, and negatively affecting the quality and consistency of the UI.

### Minimizing lock-in

The presentation logic you’ll write may end up being tightly coupled to the specific UI framework or even a component library that you use. This will make it cost-prohibitive to switch to a different component library, let alone move to a new UI framework. Given the fast-paced evolution of modern UI frameworks, this may significantly reduce the lifespan of your application, requiring a costly modernization a few years down the road.

Xomega Framework helps you to write a large part of your presentation logic in a platform-independent way, and then just bind to it a thin layer of UI views implemented with specific UI technologies. This allows you to easily switch between various UI frameworks and component libraries, and even share the same presentation logic between different types of .NET applications, such as web, desktop, or mobile.

## Framework Architecture

The generic MVVM architecture of the presentation layer built with Xomega Framework that enables its flexibility and reusability is depicted in the following diagram.

![UI architecture](../../../Xomega.Docs/blob/master/docs/framework/common-ui/img/ui_arch.png)

Communication between the presentation layer and your business services will depend on the overall architecture of your app and the specific technologies that you use for its layers, as illustrated by the following diagram.

![Services architecture](../../../Xomega.Docs/blob/master/docs/framework/services/img/services.png?raw=true)

## Documentation

Complete documentation for the Xomega Framework concepts and components with code examples is available at [Xomega.Net Docs](https://xomega.net/docs/framework/overview).

If you find any incorrect or missing documentation, please browse existing [Documentation Issues](../../../Xomega.Docs/issues) or open a new one. Also, feel free to fix any incorrect documentation and submit a PR in the [Xomega.Docs repository](../../../Xomega.Docs).

## How to get started

### VS solution wizard

The easiest and fastest way to get started with Xomega Framework is to [download and install our free VS extension](https://xomega.net/System/Download.aspx) **Xomega.Net for Visual Studio**. It provides a *New Project* template with a solution wizard, where you can configure a new solution for any supported architecture, such as Blazor, WPF, ASP.NET, or SPA, or even pick multiple architectures that will share the same code.

### Xomega low-code platform

It gets way better than that though since Xomega.Net allows you to build a domain and service model from your database, and then just **generate complete Search and Details views** with all the view models, business services, and other artifacts right from your service model. The generated code will follow Xomega Framework best practices, and all you’d have to do is to just customize it where needed.

### Walkthrough tutorial

To quickly learn how to build a Blazor Server and WebAssembly apps with our Xomega platform, give it a spin and [walk through our step-by-step tutorial](https://xomega.net/docs/tutorial/get-started), where you can build a fully functional LOB app in no time.

Alternatively, you can open the [source code for the final solution](../../../Xomega.Tutorial) that was built in that tutorial, and explore how the Xomega Framework components and code work together in Blazor apps.

### NuGet packages

Xomega Framework packages are [available via NuGet](http://www.nuget.org/packages?q=xomega.framework), so you can always manually add them to your existing projects and configure them in the application’s startup classes if you know what you’re doing.

## Community & Support
- [Community Forum](../../../Xomega.Net4VS/discussions) - browse or ask questions, suggest new features, or discuss Xomega Framework and the overall Xomega platform.
- [GitHub Issues](../../issues) - browse and report issues with Xomega Framework.
---
If you enjoy using Xomega Framework, then be sure to **add a star** ⭐ to this repository and help us **spread the word!** 📢
