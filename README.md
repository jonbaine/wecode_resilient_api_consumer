# wecode_resilient_api_consumer

.net CORE Quick and dirty example of resilient api consumer for the Wecode resilient api consumer.

We use different stragies:

 * Polly resilency framework for retries.
 * Differenciated Connect and Read timeouts.
 * Parallelism using TPL.  


To execute:
    `dotnet run`
