# EmailTracker

This webhook application tracks which email sent out was opened.

The email sent out contains an image tag in the email html.

The image tag contains a url with an email ID which will call a GET request to this webhook to return a 1x1 invisible pixel image when the email is opened by the user.

The webhook listens on GET requests and extracts the email ID from the request and stores into a database.
