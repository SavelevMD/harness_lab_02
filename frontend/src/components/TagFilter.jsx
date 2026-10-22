export default function TagFilter({ tags, value, onChange }) {
  return (
    <div className="tag-filter">
      <label htmlFor="tag-filter">Filter by tag</label>
      <select
        id="tag-filter"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      >
        <option value="All">All</option>
        {tags.map((tag) => (
          <option key={tag} value={tag}>
            {tag}
          </option>
        ))}
      </select>
    </div>
  );
}
