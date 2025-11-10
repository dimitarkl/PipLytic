# PipLytic: Advanced Trading Simulator

> **🚀 Live Demo:** [Try it now](https://piplytic.dimitarkl.me/demo) • **Live App:** [piplytic.dimitarkl.me](https://piplytic.dimitarkl.me)

## Overview

PipLytic is a full-stack trading simulator that lets users master trading strategies using real historical market data—completely risk-free. Built with modern web technologies and featuring AI-powered mentorship, it delivers a realistic trading experience with real-time data streaming, professional charting, and intelligent position management.

![Trading Feature Screenshot](images/trading-feature.png)

## Tech Stack

Built with modern technologies for performance and scalability:

**Frontend**
- React 19 with TypeScript for type-safe, concurrent UI rendering
- TailwindCSS + Radix UI for accessible, responsive design
- TradingView Lightweight Charts for professional financial visualization

**Backend**
- ASP.NET Core 9 with high-performance async/await patterns
- Entity Framework Core with PostgreSQL for robust data persistence
- JWT authentication with refresh token rotation
- Google Gemini AI integration for intelligent trading assistance

**Infrastructure**
- Automated CI/CD pipeline with GitHub Actions
- Production deployment with NGINX reverse proxy
- SSL/TLS encryption with automated certificate management
- Memory-optimized caching layer for real-time data delivery


## Key Features

### 🎯 Real-Time Trading Simulation

![Trading Feature Screenshot](images/trading-feature.png)

- **Historical Data Streaming:** Practice with real market movements from TwelveData API
- **Multiple Timeframes:** Switch between 5-minute, 15-minute, and 1-hour intervals
- **Adjustable Playback:** Control simulation speed (1x, 2x, 5x) for faster learning
- **Smart Data Chunking:** Realistic past/future data splitting mimics real trading conditions

### 💼 Professional Trading Interface
- **Full Position Management:** Execute LONG (buy) and SHORT (sell) positions
- **Flexible Position Sizing:** Quick-select 25%, 50%, 75%, or MAX of your balance
- **Live P&L Tracking:** Real-time profit/loss calculations as market data streams
- **Risk Management:** Built-in validation prevents over-leveraging
- **Starting Balance:** Begin with $1,000 virtual capital

### 🤖 AI-Powered Trading Mentor

![AI Chat Feature Screenshot](images/ai-chat-feature.png)

- **Google Gemini Integration:** Intelligent assistant powered by advanced AI
- **Context-Aware Analysis:** AI has access to your current chart data (OHLCV) up to your simulation position
- **Strategy Education:** Learn trend analysis, pattern recognition, and market dynamics
- **Persistent Chat History:** Continuous conversation throughout your trading session
- **Data Sync:** Chat session expires when market data refreshes to maintain accuracy

### 📊 Advanced Charting
- **TradingView Integration:** Professional candlestick charts with volume indicators
- **Position Markers:** Visual indicators for your active trades
- **Responsive Design:** Seamless experience on desktop, tablet, and mobile
- **Real-time Updates:** Smooth data streaming with optimized rendering

### 📈 Analytics & History

![Analytics Feature Screenshot](images/analytics-feature.png)

- **Trade History Dashboard:** Comprehensive view of all your past trades
- **Performance Metrics:** Track invested amounts, final values, and profit/loss
- **Symbol Tracking:** Monitor performance across different stocks
- **Execution Timestamps:** Detailed trade timing information

### 🔐 Secure Authentication
- **JWT-Based Security:** Industry-standard token authentication
- **Automatic Token Refresh:** Seamless session management
- **HTTP-Only Cookies:** Protected against XSS attacks
- **Password Hashing:** Secure credential storage with bcrypt

### 🎨 Modern UI/UX

![User Dashboard Screenshot](images/user-dashboard.png)

- **Dark/Light Themes:** Automatic system preference detection with manual toggle
- **Mobile-First Design:** Optimized for all screen sizes
- **Accessible Components:** Built with Radix UI primitives for WCAG compliance


## Technical Highlights

### Backend Architecture
- **ASP.NET Core 9:** Leveraging the latest .NET features for high performance
- **Entity Framework Core:** Code-first migrations with PostgreSQL for reliable data persistence
- **Custom Middleware:** Success-based rate limiting and global exception handling
- **Memory Caching Strategy:** Intelligent multi-tier caching system:
  - Shared cache for market data across users
  - Per-user cache with sliding expiration
  - Automatic cache invalidation and cleanup
- **Data Resampling:** Dynamic conversion between 5-minute, 15-minute, and 1-hour intervals
- **Concurrent Collections:** Thread-safe data structures for multi-user scenarios

### Frontend Engineering
- **React 19 Features:** Utilizing concurrent rendering for smooth UI updates
- **TypeScript Throughout:** Full type safety across the entire frontend
- **Context-Based State:** Efficient state management with UserContext, ThemeProvider, and SymbolContext
- **Custom Hooks:** Reusable logic for symbol selection, chart data, and trading operations
- **Optimized Rendering:** Memoization and effect optimization for 60fps performance
- **Form Validation:** Zod schemas with react-hook-form for robust input validation

### API Design
- **RESTful Endpoints:** Clean, predictable API structure
- **Role-Based Authorization:** Endpoint access control based on user authentication
- **Success-Based Rate Limiting:** Only counts successful requests against quotas
- **Comprehensive Error Handling:** Custom exceptions with meaningful client responses
- **CORS Configuration:** Secure cross-origin setup for production deployment

### Data Flow Innovation
- **Smart Data Splitting:** Algorithm divides historical data into "past" and "future" segments
- **Streaming Simulation:** Interval-based data revelation mimics real market conditions
- **P&L Calculations:** Real-time profit/loss computation based on current vs. entry price
- **Trade Validation:** Server-side checks prevent invalid positions and over-investment
- **AI Context Management:** Gemini receives only data up to user's current simulation point

### DevOps & Deployment
- **Automated CI/CD:** GitHub Actions pipeline for seamless deployments
- **Zero-Downtime Updates:** Rolling deployment strategy
- **SSL/TLS Security:** Automated certificate management
- **Environment Configuration:** Secure secrets management with user secrets in development
- **Database Migrations:** Automatic schema updates on deployment
- **Health Checks:** Monitoring endpoints for uptime validation

### Performance Optimizations
- **Lazy Loading:** Components and routes loaded on-demand
- **Code Splitting:** Automatic chunking for optimal bundle sizes
- **Database Indexing:** Optimized queries for fast data retrieval
- **Response Compression:** Reduced payload sizes for faster loading
- **Static Asset Caching:** Browser caching strategies for images and static files

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                         Client (React 19)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Trading    │  │  AI Chat     │  │  Analytics   │     │
│  │   Charts     │  │  Interface   │  │  Dashboard   │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
│           │                │                  │             │
│           └────────────────┴──────────────────┘             │
│                            │                                │
│                    ┌───────▼────────┐                      │
│                    │   API Client    │                      │
│                    │  (Axios + JWT)  │                      │
│                    └───────┬────────┘                      │
└────────────────────────────┼─────────────────────────────┘
                             │ HTTPS
┌────────────────────────────▼─────────────────────────────┐
│                    NGINX Reverse Proxy                    │
└────────────────────────────┬─────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────┐
│              ASP.NET Core 9 Backend                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Auth Service │  │Market Service│  │  AI Service  │  │
│  │  (JWT Auth)  │  │ (TwelveData) │  │  (Gemini)    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│           │                │                  │           │
│           └────────────────┴──────────────────┘           │
│                            │                              │
│                    ┌───────▼────────┐                    │
│                    │  Memory Cache   │                    │
│                    └───────┬────────┘                    │
│                            │                              │
│                    ┌───────▼────────┐                    │
│                    │   PostgreSQL    │                    │
│                    │   (EF Core)     │                    │
│                    └────────────────┘                    │
└──────────────────────────────────────────────────────────┘
                             │
                             ▼
                    External APIs
                  (TwelveData, Gemini)
```

## API Endpoints

### Authentication
- `POST /auth/register` - Create new user account
- `POST /auth/login` - Authenticate and receive JWT tokens
- `POST /auth/refresh-token` - Refresh expired access token
- `POST /auth/logout` - Invalidate refresh token and clear session

### User Management
- `GET /users/me` - Get current user profile and preferences
- `GET /users/trades` - Fetch user's complete trading history

### Market Data
- `POST /market/stocks/search` - Query historical stock data (creates new session)
- `POST /market/stocks/refresh` - Reset data with new random historical period
- `POST /market/stocks/continue` - Load next month of data

### Trading
- `POST /users/trades` - Open a new trading position
- `PATCH /users/trades` - Close an active position
- `GET /users/trades/{tradeId}` - Get specific trade details

### AI Assistant
- `POST /ai-chat` - Send message to Gemini AI mentor (includes chart context)
- `GET /ai-chat` - Retrieve conversation history for current session

### Companies
- `GET /companies` - List available trading symbols

### Health
- `GET /health` - Application health status and uptime

## Deployment

The application is deployed with:
- Automated CI/CD via GitHub Actions
- NGINX reverse proxy with SSL/TLS
- PostgreSQL database with automated backups
- Environment-based configuration
- Zero-downtime deployment strategy

**Live Application:** [https://piplytic.dimitarkl.me](https://piplytic.dimitarkl.me)

**Try Demo (No Sign-up):** [https://piplytic.dimitarkl.me/demo](https://piplytic.dimitarkl.me/demo)

---

## Future Enhancements

- [ ] WebSocket integration for true real-time data streaming
- [ ] More technical indicators (RSI, MACD, Bollinger Bands)
- [ ] Paper trading competitions and leaderboards
- [ ] Mobile native apps (React Native)
- [ ] Expanded stock universe beyond current majors
- [ ] Advanced order types (stop-loss, take-profit)
- [ ] Multi-currency support
- [ ] Social features (share trades, follow traders)

---

