# WorldDominationCrawler
A c# web crawler using "Dataflow" pipelines

# Setup (stand-alone)
- .NET Core 1.1 [https://www.microsoft.com/net/core]
- wdcrawler binaries [osx](http://line64.github.io/WorldDominationCrawler/osx.10.12-x64.zip)
- wdcrawler binaries [ubuntu](http://line64.github.io/WorldDominationCrawler/ubuntu.14.10-x64.zip)
- wdcrawler binaries [win10](http://line64.github.io/WorldDominationCrawler/win10-x64.zip)
- Extract .zip file and navigate to the extracted files from your terminal.

# Setup (via Docker)
- docker engine
- `docker pull scarmuega/wdcrawler:latest`
- (if using docker, remember to use `docker -it` flag to get a nice console output)

# CLI Usage

```
Stand-alone usage: dotnet wdcrawler.dll [url] [options]
via Docker usage: docker -it scarmuega/wdcrawler [url] [options]


Arguments:
  url  The seed url to crawl

Options:
  -v | --version              Show version information
  -h | --help                 Show help information
  -f | --fetch-workers <int>  Degree of parallel fetches
  -p | --parse-workers <int>  Degree of parallel parsers
  -d | --max-depth <int>      Max crawl depth from seed url
  -l | --max-links <int>      Max links to crawl per node
  -w | --parse-wait <int>     Add artificial delay (milliseconds)
```

Once the crawling process finished, the report will be uploaded to S3 and you'll be provided with a public link.

# Usage Examples
crawl "World Domination" topic on wikipedia 
```
dotnet wdcrawler.dll https://en.wikipedia.org/wiki/World_domination
```
same as above but using Docker 
```
docker run -it scarmuega/wdcrawler https://en.wikipedia.org/wiki/World_domination
```
use 4 fetch workers and 2 parse workers
```
dotnet wdcrawler.dll http://google.com --fetch-workers 4 --parse-workers 2
```
put some delay in parsing to see how the pipeline buffers the items
```
dotnet wdcrawler.dll http://google.com --parse-wait 500
```
crawl disney.com 4 levels deep 
```
dotnet wdcrawler.dll http://www.disney.com --depth 4
```

# Sample Reports
- World Domination (4 levels deep): [http://bit.ly/2ngr9U0]
- Disney website: [http://bit.ly/2ngpPQL]

# Build from Source

```
git clone git@github.com:line64/WorldDominationCrawler.git
cd WorldDominationCrawler
dotnet restore
dotnet build
```

# Code Higlights
- use of the Dataflow library to handle the crawl pipeline
- built with dotnet core (running on linux / mac / windows / docker)
- several options to tune up parallelism and performance
- AWS SDK integration (S3)
- use of dynamics to generate JSON structures
- use of generic Tuples to avoid trivial classes
- use of extension methods to extract boilerplate code from core business
- extensive use of async / await keywords
- fancy report using Javascript D3 library

# TODO
- tidy up report, show more info, provide table alternative
- memory cache to avoid re-fetch of same url
- skip links which points back to parent
- add a cancellation token to abort process but generating partial report