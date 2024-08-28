console.log(`Starting segmentation fault load tests`);
const targetUrl = 'https://YOUR-URL-HERE.amazonaws.com/v1/';
const results: Result[] = [];

main()
  .then(() => {
    console.log(`Done.`);
    const failures = results.filter((r) => r.faulty);
    console.log(`${failures.map((f) => `${f.statusCode}: ${f.error}`).join('\n')}`);
  })
  .catch((err) => {
    // Deal with the fact the chain failed
    console.log(`BOOM: ${err}`);
  });

async function main(): Promise<void> {
  console.log(`Target Url: ${targetUrl}`);

  for (let i = 0; i < 50; i++) {
    console.log(`kick off block ${i}`);
    await Promise.all([
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
      createCP(),
    ]);
    console.log(`block ${i} done`);
  }
}

async function createCP(): Promise<void> {
  let response: Response = new Response(undefined, { status: 400 });

  try {
    const requestFetchOptions = (): RequestInit => ({
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        accept: 'application/json',
      },
    });

    const startTime = Date.now();
    const fetchOptions: RequestInit = requestFetchOptions();
    console.log(`calling CP`);
    response = await fetch(targetUrl + 'document-generation', {
      ...fetchOptions,
    });
    const endTime = Date.now();
    const a = endTime - startTime;
    console.log(`CP create finished; time taken: ${a}ms`);
    const contentType = response.headers.get("content-type") ?? '';

    if (response.status >= 200 && response.status < 300) {
      results.push({ statusCode: response.status });
    } else {
      let responseText: string = '';
      if(contentType?.indexOf('json') >= 0){
        responseText = JSON.stringify(await response.json());
      }

      results.push({ statusCode: response.status, faulty: true, error: responseText });
    }
  } catch (error) {
    results.push({ statusCode: response?.status, faulty: true, error: `${error}` });
  }
}

type Result = {
  statusCode: number;
  error?: string;
  faulty?: boolean;
};
