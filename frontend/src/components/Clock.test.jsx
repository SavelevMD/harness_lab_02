import { act, render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import Clock from "./Clock";

const DATE_TIME_OPTIONS = {
  year: "numeric",
  month: "long",
  day: "numeric",
  hour: "2-digit",
  minute: "2-digit",
  second: "2-digit",
};

function expected(date) {
  return date.toLocaleString("ru-RU", DATE_TIME_OPTIONS);
}

describe("Clock", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("renders the current date and time in ru-RU locale", () => {
    const now = new Date("2026-10-24T14:35:08");
    vi.setSystemTime(now);

    render(<Clock />);

    expect(screen.getByTestId("clock")).toHaveTextContent(expected(now));
  });

  it("updates the displayed time every second", () => {
    const start = new Date("2026-10-24T14:35:08");
    vi.setSystemTime(start);

    render(<Clock />);

    expect(screen.getByTestId("clock")).toHaveTextContent(expected(start));

    act(() => {
      vi.advanceTimersByTime(1000);
    });

    const afterTick = new Date("2026-10-24T14:35:09");
    expect(screen.getByTestId("clock")).toHaveTextContent(expected(afterTick));
  });

  it("clears its interval on unmount", () => {
    vi.setSystemTime(new Date("2026-10-24T14:35:08"));

    const { unmount } = render(<Clock />);

    expect(vi.getTimerCount()).toBe(1);

    unmount();

    expect(vi.getTimerCount()).toBe(0);
  });
});
