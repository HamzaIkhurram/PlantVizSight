-- Real-Time Industrial Process Monitoring & Control Dashboard
-- Supabase PostgreSQL Schema
-- Created: 2025-11-11

-- ============================================================================
-- TAG TABLE
-- Stores tag definitions (process variables)
-- ============================================================================
CREATE TABLE IF NOT EXISTS tag (
  tag_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name TEXT UNIQUE NOT NULL,
  eu TEXT NOT NULL, -- Engineering Units (e.g., 'degC', 'bar', 'm3/h')
  scale DOUBLE PRECISION NOT NULL DEFAULT 1,
  "offset" DOUBLE PRECISION NOT NULL DEFAULT 0,
  span_low DOUBLE PRECISION NOT NULL DEFAULT 0,
  span_high DOUBLE PRECISION NOT NULL DEFAULT 100,
  datatype TEXT NOT NULL CHECK (datatype IN ('int16','uint16','float','bool')),
  read_register INT,
  write_register INT,
  site TEXT DEFAULT 'default',
  created_at TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- TAG_SAMPLE TABLE
-- Time-series data for tag values (historian)
-- ============================================================================
CREATE TABLE IF NOT EXISTS tag_sample (
  ts TIMESTAMPTZ NOT NULL,
  tag_id UUID NOT NULL REFERENCES tag(tag_id) ON DELETE CASCADE,
  value DOUBLE PRECISION NOT NULL,
  quality SMALLINT NOT NULL DEFAULT 192, -- OPC-like Good=192
  PRIMARY KEY (ts, tag_id)
);

-- ============================================================================
-- ALARM_RULE TABLE
-- Defines alarm thresholds and behaviors
-- ============================================================================
CREATE TABLE IF NOT EXISTS alarm_rule (
  alarm_rule_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  tag_id UUID REFERENCES tag(tag_id) ON DELETE CASCADE,
  type TEXT NOT NULL CHECK (type IN ('HH','H','L','LL','ROC')),
  threshold DOUBLE PRECISION,
  severity SMALLINT NOT NULL CHECK (severity BETWEEN 1 AND 4),
  hyst_pct DOUBLE PRECISION NOT NULL DEFAULT 0.5,
  delay_on_ms INT NOT NULL DEFAULT 2000,
  delay_off_ms INT NOT NULL DEFAULT 5000
);

-- ============================================================================
-- ALARM_EVENT TABLE
-- Stores alarm lifecycle events (Active, RTN, Acked)
-- ============================================================================
CREATE TABLE IF NOT EXISTS alarm_event (
  alarm_event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  alarm_rule_id UUID NOT NULL REFERENCES alarm_rule(alarm_rule_id),
  tag_id UUID NOT NULL REFERENCES tag(tag_id),
  ts_start TIMESTAMPTZ NOT NULL,
  ts_end TIMESTAMPTZ,
  state TEXT NOT NULL CHECK (state IN ('Active','RTN','Acked')),
  ack_by TEXT,
  ack_ts TIMESTAMPTZ,
  message TEXT
);

-- ============================================================================
-- AUDIT_EVENT TABLE
-- Tracks user actions (writes, acks, shelves)
-- ============================================================================
CREATE TABLE IF NOT EXISTS audit_event (
  audit_event_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_name TEXT,
  action TEXT NOT NULL,
  details JSONB,
  ts TIMESTAMPTZ DEFAULT NOW()
);

-- ============================================================================
-- INDEXES
-- Performance optimization for queries
-- ============================================================================
CREATE INDEX IF NOT EXISTS idx_tag_sample_tag_ts ON tag_sample(tag_id, ts DESC);
CREATE INDEX IF NOT EXISTS idx_alarm_event_tag_ts ON alarm_event(tag_id, ts_start DESC);
CREATE INDEX IF NOT EXISTS idx_audit_ts ON audit_event(ts DESC);
CREATE INDEX IF NOT EXISTS idx_alarm_rule_tag ON alarm_rule(tag_id);
CREATE INDEX IF NOT EXISTS idx_tag_name ON tag(name);

-- ============================================================================
-- SAMPLE DATA (Optional - for testing)
-- ============================================================================

-- Insert sample tags
INSERT INTO tag (name, eu, scale, "offset", span_low, span_high, datatype, read_register, write_register, site)
VALUES 
  ('UnitA.Temp', 'degC', 1.0, 0.0, 0.0, 200.0, 'float', 0, 100, 'default'),
  ('UnitA.Pressure', 'bar', 1.0, 0.0, 0.0, 50.0, 'float', 1, 101, 'default'),
  ('UnitA.Flow', 'm3/h', 1.0, 0.0, 0.0, 500.0, 'float', 2, 102, 'default'),
  ('UnitB.Level', '%', 1.0, 0.0, 0.0, 100.0, 'float', 3, 103, 'default')
ON CONFLICT (name) DO NOTHING;

-- Insert sample alarm rules
INSERT INTO alarm_rule (tag_id, type, threshold, severity, hyst_pct, delay_on_ms, delay_off_ms)
SELECT 
  tag_id,
  'HH' as type,
  180.0 as threshold,
  4 as severity,
  0.5 as hyst_pct,
  2000 as delay_on_ms,
  5000 as delay_off_ms
FROM tag WHERE name = 'UnitA.Temp'
ON CONFLICT DO NOTHING;

INSERT INTO alarm_rule (tag_id, type, threshold, severity, hyst_pct, delay_on_ms, delay_off_ms)
SELECT 
  tag_id,
  'H' as type,
  150.0 as threshold,
  3 as severity,
  0.5 as hyst_pct,
  2000 as delay_on_ms,
  5000 as delay_off_ms
FROM tag WHERE name = 'UnitA.Temp'
ON CONFLICT DO NOTHING;

-- ============================================================================
-- COMMENTS
-- ============================================================================
COMMENT ON TABLE tag IS 'Process variable definitions';
COMMENT ON TABLE tag_sample IS 'Time-series historian data';
COMMENT ON TABLE alarm_rule IS 'Alarm threshold and behavior definitions';
COMMENT ON TABLE alarm_event IS 'Alarm lifecycle events';
COMMENT ON TABLE audit_event IS 'User action audit trail';

