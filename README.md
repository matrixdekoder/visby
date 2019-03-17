# Silobreaker Challenge - Code Review of API

This package contains source files and xml configuration from a part of the Silobreaker API.
It is by no means compilable or complete, but it shows a small section of our code base.

Your task is to review the code and configuration and try to understand as much
as possible, and then answer our questions.

* Start by viewing the configurations in spring.xml.
* In spring.xml, you can see how DynamicController is configured to handle requests to certain endpoints in our API.
* One of them is called termsFromDocuments, which is an instance of a ItemsFromDocumentsProcessor.
* ItemsFromDocumentsProcessor uses a couple of other classes, such parameterResolver.

## Questions

1) Can you think of a reason for DynamicController to be designed as it is?

2) Can you think of a better way to pass query parameters to the invoked methods,
   which is currently handled by the parameterResolver?

3) If you were to refactor the current code, what kind of architecture would you suggest?

Feel free to ask follow-up questions if you feel anything is insufficiently described/missing.
