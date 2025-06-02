CREATE TABLE IF NOT EXISTS habit_trackings (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL,
    habit_id UUID NOT NULL,
    tracked_date DATE NOT NULL,
    note TEXT,
    UNIQUE (user_id, habit_id, tracked_date)
);