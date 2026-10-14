import { useNavigate } from 'react-router-dom';
import './HeroBlock.css';

function HeroBlock() {
  const navigate = useNavigate();

  return (
    <section className="hero">
      <div className="hero-content">
        <h1 className="hero-title">Фотоцентр</h1>
        <p className="hero-subtitle">Творим шедевры вместе</p>
        <p className="hero-text">
          Профессиональные фотоуслуги и фототовары для вас и вашей семьи.
          Печать, съёмка, ретушь — всё в одном месте.
        </p>
        <div className="hero-buttons">
          <button className="hero-btn primary" onClick={() => navigate('/services')}>
            Услуги
          </button>
          <button className="hero-btn secondary" onClick={() => navigate('/products')}>
            Товары
          </button>
        </div>
      </div>
    </section>
  );
}

export default HeroBlock;