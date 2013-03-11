sync.push/pull can be run with two different remote file stores:
-a file system store
-a parse.com based store in the cloud

To use the parse.com cloud store be sure to create a file called .syncconfig and put it in a folder called "unversioned" (or put it in the local repo folder).
The content of .syncconfig has to be structured like this:

<parse.com App Id>
<parse.com REST Key>
<parse.com Master Key>