import { Link } from 'react-router-dom';
import './Footer.css';

function Footer() {
  return (
    <footer className="footer">
      <div className="footer-container">
        <div className="footer-section">
          <h3>Фотоцентр</h3>
          <p>© 2026 Фотоцентр. Все права защищены.</p>
        </div>

        <div className="footer-section">
          <h4>Контакты</h4>
          <p>Телефон: +7 (999) 123-45-67</p>
          <p>Email: info@photocenter.ru</p>
        </div>

        <div className="footer-section">
          <h4>Навигация</h4>
          <Link to="/" className="footer-link">Главная</Link>
          <Link to="/services" className="footer-link">Услуги</Link>
          <Link to="/products" className="footer-link">Товары</Link>
        </div>
      </div>
    </footer>
  );
}

export default Footer;