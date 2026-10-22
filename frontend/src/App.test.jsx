import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import App from "./App";
import { createNote, deleteNote, getNotes, getNotesByTag } from "./api";

vi.mock("./api", () => ({
  getNotes: vi.fn(),
  getNotesByTag: vi.fn(),
  createNote: vi.fn(),
  deleteNote: vi.fn(),
}));

const sampleNotes = [
  { id: 1, title: "First", text: "Hello", tag: "work" },
  { id: 2, title: "Second", text: "", tag: "personal" },
  { id: 3, title: "Third", text: "Ideas", tag: "work" },
];

describe("Notes with tags UI", () => {
  let user;

  beforeEach(() => {
    user = userEvent.setup();
    vi.resetAllMocks();
    getNotes.mockResolvedValue([]);
    getNotesByTag.mockResolvedValue([]);
    createNote.mockResolvedValue({ id: 999, title: "", text: "", tag: "" });
    deleteNote.mockResolvedValue(undefined);
  });

  it("renders the creation form with title, text and tag fields", async () => {
    render(<App />);

    expect(screen.getByLabelText("Title")).toBeInTheDocument();
    expect(screen.getByLabelText("Text")).toBeInTheDocument();
    expect(screen.getByLabelText("Tag")).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Add note" })
    ).toBeInTheDocument();

    // allow the initial fetch to settle so the test doesn't leak state updates
    await screen.findByText("No notes found.");
  });

  it("shows a loading state while notes are being fetched", async () => {
    let resolveGet;
    getNotes.mockImplementation(
      () => new Promise((resolve) => (resolveGet = resolve))
    );

    render(<App />);

    expect(screen.getByRole("status")).toHaveTextContent("Loading notes");

    resolveGet(sampleNotes);
    await screen.findByText("First");
  });

  it("renders the list of notes with title, text and tag", async () => {
    getNotes.mockResolvedValue(sampleNotes);
    render(<App />);

    const first = await screen.findByTestId("note-1");
    expect(within(first).getByText("First")).toBeInTheDocument();
    expect(within(first).getByText("Hello")).toBeInTheDocument();
    expect(within(first).getByText("work")).toBeInTheDocument();

    // a note without text still renders its title and tag
    const second = screen.getByTestId("note-2");
    expect(within(second).getByText("Second")).toBeInTheDocument();
    expect(within(second).getByText("personal")).toBeInTheDocument();
  });

  it("renders a delete button per note", async () => {
    getNotes.mockResolvedValue(sampleNotes);
    render(<App />);
    await screen.findByTestId("note-1");

    expect(
      screen.getByRole("button", { name: "Delete note First" })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Delete note Second" })
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Delete note Third" })
    ).toBeInTheDocument();
  });

  it("deletes a note and refreshes the list without a reload", async () => {
    getNotes
      .mockResolvedValueOnce(sampleNotes)
      .mockResolvedValueOnce([sampleNotes[1], sampleNotes[2]]);

    render(<App />);
    await screen.findByText("First");

    await user.click(screen.getByRole("button", { name: "Delete note First" }));

    await waitFor(() => {
      expect(deleteNote).toHaveBeenCalledWith(1);
    });

    // list re-fetched and the deleted note is gone
    await waitFor(() => {
      expect(screen.queryByText("First")).not.toBeInTheDocument();
    });
    expect(screen.getByText("Second")).toBeInTheDocument();
    expect(getNotes).toHaveBeenCalledTimes(2);
  });

  it("populates the tag filter with distinct tags plus an All option", async () => {
    getNotes.mockResolvedValue(sampleNotes);
    render(<App />);
    await screen.findByTestId("note-1");

    const select = screen.getByLabelText("Filter by tag");
    const options = within(select).getAllByRole("option");
    const values = options.map((o) => o.value);

    expect(values).toEqual(["All", "work", "personal"]);
  });

  it("re-fetches filtered notes when a tag is selected", async () => {
    getNotes.mockResolvedValue(sampleNotes);
    getNotesByTag.mockResolvedValue([sampleNotes[0], sampleNotes[2]]);

    render(<App />);
    await screen.findByText("Second");

    await user.selectOptions(screen.getByLabelText("Filter by tag"), "work");

    await waitFor(() => {
      expect(getNotesByTag).toHaveBeenCalledWith("work");
    });

    await waitFor(() => {
      expect(screen.queryByText("Second")).not.toBeInTheDocument();
    });
    expect(screen.getByText("First")).toBeInTheDocument();
    expect(screen.getByText("Third")).toBeInTheDocument();
  });

  it("disables submit until title and tag are provided", async () => {
    render(<App />);
    await screen.findByText("No notes found.");

    const button = screen.getByRole("button", { name: "Add note" });
    expect(button).toBeDisabled();

    await user.type(screen.getByLabelText("Title"), "My note");
    expect(button).toBeDisabled(); // tag still empty

    await user.type(screen.getByLabelText("Tag"), "work");
    expect(button).toBeEnabled();
  });

  it("shows validation messages when required fields are left empty", async () => {
    render(<App />);
    await screen.findByText("No notes found.");

    await user.click(screen.getByLabelText("Title"));
    await user.tab(); // blur the title input

    expect(screen.getByRole("alert")).toHaveTextContent("Title is required");
  });

  it("creates a note and refreshes the list without a reload", async () => {
    const created = { id: 4, title: "New", text: "Body", tag: "ideas" };
    getNotes
      .mockResolvedValueOnce(sampleNotes)
      .mockResolvedValueOnce([...sampleNotes, created]);

    render(<App />);
    await screen.findByText("First");

    await user.type(screen.getByLabelText("Title"), "New");
    await user.type(screen.getByLabelText("Text"), "Body");
    await user.type(screen.getByLabelText("Tag"), "ideas");
    await user.click(screen.getByRole("button", { name: "Add note" }));

    await waitFor(() => {
      expect(createNote).toHaveBeenCalledWith({
        title: "New",
        text: "Body",
        tag: "ideas",
      });
    });

    await screen.findByText("New");
    expect(screen.getByText("Body")).toBeInTheDocument();
    expect(getNotes).toHaveBeenCalledTimes(2);
  });

  it("shows an error message when the API is unreachable", async () => {
    getNotes.mockRejectedValue(new Error("Network error"));
    render(<App />);

    const alert = await screen.findByRole("alert");
    expect(alert).toHaveTextContent("Network error");
  });

  it("shows an error message when deleting a missing note", async () => {
    getNotes.mockResolvedValue(sampleNotes);
    deleteNote.mockRejectedValue(new Error("Note not found"));

    render(<App />);
    await screen.findByText("First");

    await user.click(screen.getByRole("button", { name: "Delete note First" }));

    const alert = await screen.findByRole("alert");
    expect(alert).toHaveTextContent("Note not found");
  });
});
