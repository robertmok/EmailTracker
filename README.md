# EmailTracker

This webhook application tracks which email sent out was opened.

The email sent out contains a image tag in the email html.

The image tag contains a url with an email ID which will call a get request to this webhook to return a 1x1 invisible pixel image.

The webhook listens on get requests and extracts the email ID from the request and stores into a database.
