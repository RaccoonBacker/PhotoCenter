import './Placeholder.css';

function Placeholder({ text = 'Остальное будет реализовано позднее' }) {
  return (
    <div className="placeholder">
      <p>{text}</p>
    </div>
  );
}

export default Placeholder;