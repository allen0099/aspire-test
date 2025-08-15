import { useState } from "react";
import { Button } from "@heroui/button";
import { Code } from "@heroui/code";
import { title } from "@/components/primitives";
import DefaultLayout from "@/layouts/default";

export default function TestApiPage() {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleClick = async () => {
    setLoading(true);
    try {
      const response = await fetch("/api/users/test");
      const result = await response.json();
      setData(result);
    } catch (error) {
      console.error("Error fetching data:", error);
      setData(error);
    }
    setLoading(false);
  };

  return (
    <DefaultLayout>
      <section className="flex flex-col items-center justify-center gap-4 py-8 md:py-10">
        <div className="inline-block max-w-lg text-center justify-center">
          <h1 className={title()}>Test API Connection</h1>
        </div>

        <div className="flex gap-3">
          <Button isLoading={loading} onClick={handleClick}>
            Call API
          </Button>
        </div>

        {data && (
          <div className="mt-4">
            <Code className="w-full" size="lg">
              {JSON.stringify(data, null, 2)}
            </Code>
          </div>
        )}
      </section>
    </DefaultLayout>
  );
}
