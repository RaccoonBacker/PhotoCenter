import './Tabs.css';

function Tabs({ tabs, activeTab, onChange }) {
  return (
    <div className="admin-tabs" role="tablist" aria-label="Разделы панели администратора">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          type="button"
          role="tab"
          aria-selected={activeTab === tab.id}
          className={`admin-tab ${activeTab === tab.id ? 'admin-tab-active' : ''}`}
          onClick={() => onChange(tab.id)}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}

export default Tabs;
