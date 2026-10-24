import { useEffect, useState } from "react";

const DATE_TIME_OPTIONS = {
  year: "numeric",
  month: "long",
  day: "numeric",
  hour: "2-digit",
  minute: "2-digit",
  second: "2-digit",
};

function formatDateTime(date) {
  return date.toLocaleString("ru-RU", DATE_TIME_OPTIONS);
}

export default function Clock() {
  const [now, setNow] = useState(() => new Date());

  useEffect(() => {
    const id = setInterval(() => {
      setNow(new Date());
    }, 1000);

    return () => clearInterval(id);
  }, []);

  return (
    <time className="clock" dateTime={now.toISOString()} data-testid="clock">
      {formatDateTime(now)}
    </time>
  );
}
